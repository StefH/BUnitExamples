using Microsoft.AspNetCore.Components;

namespace BlazorApp.WebAssembly.Components
{
    public partial class CounterDark
    {
        [CascadingParameter]
        public bool IsDark { get; set; }

        [Parameter]
        public int Start { get; set; } = 0;

        private void Minus()
        {
            Start--;
        }
    }
}
