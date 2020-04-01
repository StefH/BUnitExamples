using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bunit;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Components;
using Moq;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

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

            public IRenderedComponent<TComponent> RenderComponent6<TComponent>(params Action<TComponent>[] actions) where TComponent : class, IComponent, new()
            {
                var componentParameters = new List<ComponentParameter>();

                foreach (var action in actions)
                {
                    var orig = new TComponent();
                    var n = new TComponent();

                    action.Invoke(n);

                    var name = action.Method.Name;
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
