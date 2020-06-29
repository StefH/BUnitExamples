using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorApp.WebAssembly.Components
{
    public class Wrapper : ComponentBase
    {
        [Parameter]
        public bool IsDark { get; set; } = true;


        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "style", IsDark ? "background-color: darkgrey;" : "");

            builder.OpenElement(2, "p");
            builder.AddContent(3, $"IsDark = {IsDark}");
            builder.CloseElement();

            builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }
    }
}
