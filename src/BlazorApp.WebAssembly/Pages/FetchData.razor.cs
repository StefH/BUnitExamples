using System.Threading.Tasks;
using BlazorApp.WebAssembly.Models;
using BlazorApp.WebAssembly.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.WebAssembly.Pages
{
    public partial class FetchData
    {
        [Inject]
        public IWeatherForecastService Service { get; set; }

        public WeatherForecast[] Forecasts;

        protected override async Task OnInitializedAsync()
        {
            Forecasts = await Service.GetAsync();
        }
    }
}