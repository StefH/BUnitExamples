using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorApp.WebAssembly.Components
{
    public partial class Wrapper : ComponentBase
    {
        [Parameter]
        public bool IsDark { get; set; } = true;


        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, ChildContent);
    }
}
