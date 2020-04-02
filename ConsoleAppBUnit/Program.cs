using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bunit;
using Castle.DynamicProxy;
using ChangeTracking;
using FakeItEasy;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using IInvocation = Moq.IInvocation;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ConsoleAppBUnit
{
    public class Program
    {
        static void Main(string[] args)
        {
            var x = new X();

            var r = x.RenderComponent<Com>(ComponentParameter.CreateParameter("Age", 42));

            var r1 = x.RenderComponent<Com>(
                 c => c.Name == "x",
                 c => c.Age == 42
                 //c => c.NonGenericCallback == EventCallback.Empty   <<< does not work ; so this interface cannot be used...
             );

            //var di = new ServiceCollection();
            //di.AddLogging(lb => lb.AddFilter(f => f == LogLevel.Trace));

            // With a sort of builder-pattern
            var r2 = x.RenderComponent2<Com>(cb =>
            {
                var c = new Com
                {
                    Age = 42,
                    Name = "n",
                    NonGenericCallback = EventCallback.Empty
                };

                cb.Set(c);
            });

            // Just provide a new instance from the component and copy these values to parameters
            var r3 = x.RenderComponent3(new Com
            {
                Age = 42,
                Name = "n",
                NonGenericCallback = EventCallback.Empty
            });

            // Just provide an action to set some values to a new component and copy these values to parameters
            var r4 = x.RenderComponent4<Com>(c =>
            {
                c.Name = "x";
                c.Age = 42;
                c.NonGenericCallback = EventCallback.Empty;
                c.GenericCallback = new EventCallback<EventArgs>();
            });


            var builder = new ComponentParameterTypedBuilder<Com>();
            builder.Set(c => c.Name, "name");
            builder.Set(c => c.Age, 42);

            var r5 = x.RenderComponent5(builder);

            var products = new Builder()
                .CreateListOfSize<Com>(10)
                .IndexOf(0)
                .With(xxx => xxx.Name = "A special title")
                .Build();

            var r6 = x.RenderComponent6<Com>(
                c => c.Name = "stef",
                c => c.Age = 42
            );

            var r7 = x.RenderComponent<Com>(
                ComponentParameterTyped<Com>.Create(c => c.Name, "n"),
                ComponentParameterTyped<Com>.Create(c => c.Age, 3)
            );
        }

        public static T Set<T, TProp>(T o,
            Expression<Func<T, TProp>> field, TProp value)
        {
            var fn = ((MemberExpression)field.Body).Member.Name;
            //o.GetType().GetProperty(fn).SetValue(o, value, null);
            return o;
        }

        public class ComponentParameterTypedBuilder<TComponent> where TComponent : class, IComponent
        {
            private readonly List<ComponentParameter> _componentParameters = new List<ComponentParameter>();

            public void Set<TValue>(Expression<Func<TComponent, TValue>> expression, TValue value)
            {
                if (expression.Body is MemberExpression memberExpression)
                {
                    string name = memberExpression.Member.Name;
                    _componentParameters.Add(ComponentParameter.CreateParameter(name, value));
                }
            }

            public ComponentParameter[] Build()
            {
                return _componentParameters.ToArray();
            }
        }

        public class ComponentParameterTyped<TComponent> where TComponent : class, IComponent
        {
            public static ComponentParameterTyped<TComponent> Createx(Expression<Func<TComponent, object>> expression, object value) //where TComponent : class, IComponent
            {
                return null;
            }

            public static ComponentParameter Create<TValue>(Expression<Func<TComponent, TValue>> expression, TValue value) //where TComponent : class, IComponent
            {
                if (expression.Body is MemberExpression memberExpression)
                {
                    string name = memberExpression.Member.Name;
                    return ComponentParameter.CreateParameter(name, value);
                }
                throw new Exception();
            }

            //public static ComponentParameterTyped<TComponent> Create<TValue>(Expression<Func<TComponent, TValue>> expression, TValue value) //where TComponent : class, IComponent
            //{
            //    if (expression.Body is MemberExpression memberExpression)
            //    {
            //        string name = memberExpression.Member.Name;
            //        return ComponentParameter.CreateParameter(name, value);
            //    }
            //    throw new Exception();
            //}
        }

        public class ComponentBuilder<TComponent> where TComponent : class, IComponent
        {
            public TComponent Component { get; private set; }

            public void Set(TComponent c)
            {
                Component = c;
            }

            public ComponentParameter[] ToComponentParameters()
            {
                return null;
            }
        }

        class X : TestComponentBase
        {
            public IRenderedComponent<TComponent> RenderComponent<TComponent>(params Expression<Func<TComponent, object>>[] parameters) where TComponent : class, IComponent
            {
                var componentParameters = new List<ComponentParameter>();
                foreach (var parameter in parameters)
                {
                    if (parameter.Body is UnaryExpression body)
                    {
                        if (body.Operand is BinaryExpression methodBinaryExpression)
                        {
                            if (methodBinaryExpression.Left is MemberExpression left && methodBinaryExpression.Right is ConstantExpression right)
                            {
                                var name = left.Member.Name;
                                var value = right.Value;

                                componentParameters.Add(ComponentParameter.CreateParameter(name, value));
                            }

                        }
                    }
                }

                return base.RenderComponent<TComponent>(componentParameters.ToArray());
            }

            public IRenderedComponent<TComponent> RenderComponent2<TComponent>(Action<ComponentBuilder<TComponent>> a) where TComponent : class, IComponent, new()
            {
                var componentParameters = new List<ComponentParameter>();

                var cb = new ComponentBuilder<TComponent>();
                a.Invoke(cb);

                foreach (var p in typeof(TComponent).GetProperties())
                {
                    componentParameters.Add(ComponentParameter.CreateParameter(p.Name, p.GetValue(cb.Component)));
                }

                return base.RenderComponent<TComponent>(componentParameters.ToArray());
            }

            public IRenderedComponent<TComponent> RenderComponent3<TComponent>(TComponent c) where TComponent : class, IComponent
            {
                var componentParameters = new List<ComponentParameter>();
                foreach (var p in c.GetType().GetProperties())
                {
                    componentParameters.Add(ComponentParameter.CreateParameter(p.Name, p.GetValue(c)));
                }

                return base.RenderComponent<TComponent>(componentParameters.ToArray());
            }

            public IRenderedComponent<TComponent> RenderComponent4<TComponent>(Action<TComponent> a) where TComponent : class, IComponent, new()
            {
                var componentParameters = new List<ComponentParameter>();

                var component = new TComponent();
                a.Invoke(component);

                foreach (var p in typeof(TComponent).GetProperties())
                {
                    componentParameters.Add(ComponentParameter.CreateParameter(p.Name, p.GetValue(component)));
                }

                return base.RenderComponent<TComponent>(componentParameters.ToArray());
            }

            public IRenderedComponent<TComponent> RenderComponent5<TComponent>(ComponentParameterTypedBuilder<TComponent> builder) where TComponent : class, IComponent
            {
                return base.RenderComponent<TComponent>(builder.Build());
            }

            public class Variance
            {
                public string Prop { get; set; }
                public object Left { get; set; }
                public object Right { get; set; }
            }

            public static List<Variance> DetailedCompare<T>(T left, T right)
            {
                List<Variance> variances = new List<Variance>();
                var fi = left.GetType().GetProperties();
                foreach (var f in fi)
                {
                    var v = new Variance
                    {
                        Prop = f.Name,
                        Left = f.GetValue(left),
                        Right = f.GetValue(right)
                    };

                    if (!Equals(v.Left, v.Right))
                    {
                        variances.Add(v);
                    }

                }
                return variances;
            }

            private static Expression<Func<T>> CreatePropertyGetterExpression<T>(PropertyInfo property)
            {
                Type entityType = typeof(T);
                var parameter = Expression.Parameter(entityType, "e");
                Type funcType = typeof(Func<>).MakeGenericType(typeof(T));
                //var body = Expression.MakeMemberAccess(parameter, property);
                return (Expression<Func<T>>)Expression.Lambda(funcType, parameter);
            }

            public static Expression<Func<T>> Get<T>(PropertyInfo pInfo)
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T));
                MemberExpression property = Expression.Property(parameter, pInfo);
                Type funcType = typeof(Func<>).MakeGenericType(typeof(T));

                //next line fails: can't convert int to object

                LambdaExpression lambda;
                if (typeof(T).IsClass == false && typeof(T).IsInterface == false)
                    lambda = Expression.Lambda(funcType, Expression.Convert(property, typeof(Object)), parameter);
                else
                    lambda = Expression.Lambda(funcType, property, parameter);

                return (Expression<Func<T>>)lambda;
            }

            //private static TypeBuilder CreateImplementationType(string name, PropertyInfo[] properties)
            //{
            //    TypeBuilder typeBuilder = moduleBuilder.DefineType(name,
            //        TypeAttributes.Public |
            //        TypeAttributes.Class |
            //        TypeAttributes.AutoClass |
            //        TypeAttributes.AnsiClass |
            //        TypeAttributes.AutoLayout,
            //        typeof(object));

            //    foreach (PropertyInfo pi in properties)
            //    {
            //        string piName = pi.Name;
            //        Type propertyType = pi.PropertyType;

            //        FieldBuilder field = typeBuilder.DefineField("_" + piName, propertyType, FieldAttributes.Private);

            //        MethodInfo getMethod = pi.GetGetMethod();
            //        if (getMethod != null)
            //        {
            //            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
            //                getMethod.Name,
            //                MethodAttributes.Public |
            //                MethodAttributes.SpecialName |
            //                MethodAttributes.Virtual,
            //                propertyType,
            //                Type.EmptyTypes);

            //            var ilGenerator = methodBuilder.GetILGenerator();
            //            ilGenerator.Emit(OpCodes.Ldarg_0); // Load "this"
            //            ilGenerator.Emit(OpCodes.Ldfld, field); // Load the backing field
            //            ilGenerator.Emit(OpCodes.Ret); // Return the value on the stack (field)

            //            // Override the get method from the original type to this IL-generated version
            //            typeBuilder.DefineMethodOverride(methodBuilder, getMethod);
            //        }

            //        var setMethod = pi.GetSetMethod();
            //        if (setMethod != null)
            //        {
            //            var methodBuilder = typeBuilder.DefineMethod(
            //                setMethod.Name,
            //                MethodAttributes.Public |
            //                MethodAttributes.SpecialName |
            //                MethodAttributes.Virtual,
            //                typeof(void),
            //                new Type[] { pi.PropertyType });

            //            var ilGenerator = methodBuilder.GetILGenerator();
            //            ilGenerator.Emit(OpCodes.Ldarg_0); // Load "this"
            //            ilGenerator.Emit(OpCodes.Ldarg_1); // Load the set value
            //            ilGenerator.Emit(OpCodes.Stfld, field); // Set the field to the set value
            //            ilGenerator.Emit(OpCodes.Ret);

            //            // Override the set method from the original type to this IL-generated version
            //            typeBuilder.DefineMethodOverride(methodBuilder, setMethod);
            //        }
            //    }

            //    return typeBuilder;
            //}

            public class Interceptor : IInterceptor
            {
                public void Intercept(IInvocation invocation)
                {

                }

                public void Intercept(Castle.DynamicProxy.IInvocation invocation)
                {
                    Console.WriteLine($"Before target call {invocation.Method.Name}");
                    try
                    {
                        invocation.Proceed();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Target exception {e.Message}");
                        throw;
                    }
                    finally
                    {
                        Console.WriteLine($"After target call {invocation.Method.Name}");
                    }
                }
            }


            public class MyClassBuilder
            {
                AssemblyName asemblyName;
                public MyClassBuilder(string className)
                {
                    asemblyName = new AssemblyName(className);
                }
                public object CreateObject(string[] propertyNames, Type[] types)
                {
                    if (propertyNames.Length != types.Length)
                    {
                        Console.WriteLine("The number of property names should match their corresopnding types number");
                    }

                    TypeBuilder dynamicClass = CreateClass();
                    CreateConstructor(dynamicClass);
                    for (int ind = 0; ind < propertyNames.Count(); ind++)
                        CreateProperty(dynamicClass, propertyNames[ind], types[ind]);
                    Type type = dynamicClass.CreateType();

                    return Activator.CreateInstance(type);
                }
                private TypeBuilder CreateClass()
                {
                    AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asemblyName, AssemblyBuilderAccess.Run);
                    ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
                    TypeBuilder typeBuilder = moduleBuilder.DefineType(asemblyName.FullName
                                        , TypeAttributes.Public |
                                        TypeAttributes.Class |
                                        TypeAttributes.AutoClass |
                                        TypeAttributes.AnsiClass |
                                        TypeAttributes.BeforeFieldInit |
                                        TypeAttributes.AutoLayout
                                        , null);
                    return typeBuilder;
                }
                private void CreateConstructor(TypeBuilder typeBuilder)
                {
                    typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
                }
                private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
                {
                    FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

                    PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
                    MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, propertyType, Type.EmptyTypes);
                    ILGenerator getIl = getPropMthdBldr.GetILGenerator();

                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Ldfld, fieldBuilder);
                    getIl.Emit(OpCodes.Ret);

                    MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                          MethodAttributes.Public |
                          MethodAttributes.SpecialName |
                          MethodAttributes.HideBySig,
                          null, new[] { propertyType });

                    ILGenerator setIl = setPropMthdBldr.GetILGenerator();
                    Label modifyProperty = setIl.DefineLabel();
                    Label exitSet = setIl.DefineLabel();

                    setIl.MarkLabel(modifyProperty);
                    setIl.Emit(OpCodes.Ldarg_0);
                    setIl.Emit(OpCodes.Ldarg_1);
                    setIl.Emit(OpCodes.Stfld, fieldBuilder);

                    setIl.Emit(OpCodes.Nop);
                    setIl.MarkLabel(exitSet);
                    setIl.Emit(OpCodes.Ret);

                    propertyBuilder.SetGetMethod(getPropMthdBldr);
                    propertyBuilder.SetSetMethod(setPropMthdBldr);
                }
            }


            public Action<object> Convert<T>(Action<T> myActionT)
            {
                if (myActionT == null)
                {
                    return null;
                }

                return o => myActionT((T)o);
            }

            public IRenderedComponent<TComponent> RenderComponent6<TComponent>(params Action<TComponent>[] actions) where TComponent : class, IComponent, new()
            {
                var componentParameters = new List<ComponentParameter>();
                //var ccc = new Com();
                foreach (var action in actions)
                {
                    var props = typeof(TComponent).GetProperties();
                    foreach (var p in props)
                    {
                        //var ex = CreatePropertyGetterExpression(typeof(TComponent), p);
                        var ex2 = CreatePropertyGetterExpression<TComponent>(p);
                        // A.CallToSet(() => fc2.Name)).i
                    }
                    var mc = (object)new MyClassBuilder(typeof(TComponent).Name + "Dynamic").CreateObject(props.Select(p => p.Name).ToArray(), props.Select(p => p.PropertyType).ToArray());

                    var ast = new TComponent();
                    //var ast2 = ast.AsTrackable();

                    //var trackable = ast2.CastToIChangeTrackable();

                    var cp = (TComponent)new ProxyGenerator().CreateClassProxy(typeof(TComponent), new Type[0],
                        new ProxyGenerationOptions
                        {

                        });
                    //var ct = new ProxyGenerator().CreateInterfaceProxyWithTarget()

                    //var cp  = order.AsTrackable();


                    //var fc2 = A.Fake<Com>();
                    var fc = A.Fake<TComponent>();

                    // Use the conventional .NET prefix "set_" to refer to a property's setter:
                    A.CallTo(fc).Where(call => call.Method.Name != "set_Address")
                        .Throws(new Exception("we can't move"));

                    A.CallTo(fc).Where(call => call != null).Invokes(x =>
                    {
                        int xxxx = 9;
                    });
                    // .Throws(new Exception("we can't move"));





                    var orig = new TComponent();
                    var n = new TComponent();

                    //var action2 = Convert(action);

                    //action2.Invoke(mc);
                    action.Invoke(n);

                    //cp.Execute();

                    var calls = Fake.GetCalls(fc).ToList();
                    var man = Fake.GetFakeManager(fc);


                    //var name = action.Method.Name;
                    var com = DetailedCompare(orig, n);

                    var variance = com.FirstOrDefault();
                    if (variance != null)
                    {
                        componentParameters.Add(ComponentParameter.CreateParameter(variance.Prop, variance.Left ?? variance.Right));
                    }
                }

                return base.RenderComponent<TComponent>(componentParameters.ToArray());




                ////var m = new Mock<TComponent>();
                //var m = NSubstitute.Substitute.For<TComponent>();

                ////var t = new TComponent();
                //actions[0].Invoke(n);



                ////m.VerifySet(actions[0], Times.Once);

                //var aaa = m.ReceivedWithAnyArgs(Quantity.AtLeastOne());


                //var xxxx = m.Received();
                //var f = typeof(TComponent).GetProperties().Where(p => p.Name == "Name").First();


                //var calls = m.ReceivedCalls();

                ////m.

                //return null; //base.RenderComponent<TComponent>(builder.Build());
            }

            public IRenderedComponent<TComponent> RenderComponent7<TComponent>(params ComponentParameterTyped<TComponent>[] parameters) where TComponent : class, IComponent
            {
                return null;// base.RenderComponent<TComponent>(builder.Build());
            }
        }

        public class Com : IComponent
        {
            [Parameter]
            public string Name { get; set; }

            [Parameter]
            public int Age { get; set; }

            [Parameter]
            public EventCallback NonGenericCallback { get; set; }

            [Parameter]
            public EventCallback<EventArgs> GenericCallback { get; set; }

            [Parameter]
            public RenderFragment ChildContent { get; set; }

            public void Attach(RenderHandle renderHandle)
            {
                //throw new NotImplementedException();
            }

            public Task SetParametersAsync(ParameterView parameters)
            {
                return Task.CompletedTask; // throw new NotImplementedException();
            }
        }
    }
}
