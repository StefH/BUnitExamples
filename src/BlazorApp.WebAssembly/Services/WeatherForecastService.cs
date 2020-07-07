using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.WebAssembly.Models;

namespace BlazorApp.WebAssembly.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient _client;

        public WeatherForecastService(HttpClient client)
        {
            _client = client;
        }

        public Task<WeatherForecast[]> GetAsync()
        {
            return _client.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
        }
    }
}