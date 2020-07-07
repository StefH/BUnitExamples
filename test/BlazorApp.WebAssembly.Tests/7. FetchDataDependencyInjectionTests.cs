using System;
using BlazorApp.WebAssembly.Models;
using BlazorApp.WebAssembly.Pages;
using BlazorApp.WebAssembly.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BlazorApp.WebAssembly.Tests
{
    public class FetchDataDependencyInjectionTests : TestContext
    {
        private readonly Mock<IWeatherForecastService> _weatherForecastServiceMock;

        public FetchDataDependencyInjectionTests()
        {
            _weatherForecastServiceMock = new Mock<IWeatherForecastService>();

            var forecasts = new[] { new WeatherForecast { Date = new DateTime(2020, 7, 7), Summary = "Test123", TemperatureC = 42 } };
            _weatherForecastServiceMock.Setup(w => w.GetAsync()).ReturnsAsync(forecasts);
        }

        [Fact]
        public void Test001()
        {
            // Arrange - add the mock forecast service
            Services.AddSingleton(_weatherForecastServiceMock.Object);

            // Act - render the FetchData component
            var fetchDataComponent = RenderComponent<FetchData>();

            // Assert that it renders the initial loading message
            var initialExpectedHtml = @"<h1>Weather forecast</h1>
<p>This component demonstrates fetching data from the server.</p>
<table class=""table"">
  <thead>
    <tr>
      <th>Date</th>
      <th>Temp. (C)</th>
      <th>Temp. (F)</th>
      <th>Summary</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>7/7/2020</td>
      <td>42</td>
      <td>107</td>
      <td>Test123</td>
    </tr>
  </tbody>
</table>";
            fetchDataComponent.MarkupMatches(initialExpectedHtml);
        }
    }
}