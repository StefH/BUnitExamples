using Microsoft.AspNetCore.Components;

namespace BlazorApp.WebAssembly.Components
{
    public partial class SpecialCounter : ComponentBase
    {
        [Parameter]
        public int Start { get; set; } = 0;

        private void Minus()
        {
            Start--;
        }
    }
}