using System.Threading.Tasks;
using BlazorApp.WebAssembly.Models;

namespace BlazorApp.WebAssembly.Services
{
    public interface IWeatherForecastService
    {
        Task<WeatherForecast[]> GetAsync();
    }
}