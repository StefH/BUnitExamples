using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace ConsoleAppBUnit
{
    class Program
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
        }

        class Com : IComponent
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
