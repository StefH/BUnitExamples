using BlazorApp.WebAssembly.Components;
using BlazorApp.WebAssembly.Pages;
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorApp.WebAssembly.Tests
{
    public class SpecialCounterTests : TestContext
    {
        [Fact]
        public void CounterShouldUseStart()
        {
            // Arrange / Act: render the SpecialCounter.razor component
            var counterComponent = RenderComponent<SpecialCounter>(("Start", 3));

            // Assert: first find the <p> element, then verify its content
            counterComponent.Find("p").MarkupMatches("<p>Current count: 3</p>");
        }

        [Fact]
        public void CounterClickShouldDecreaseValue()
        {
            // Arrange: render the SpecialCounter.razor component
            var counterComponent = RenderComponent<SpecialCounter>(("Start", 3));

            // Act: find and click the <button> element to decrease value
            // the counter in the <p> element
            counterComponent.Find("button").Click();

            // Assert: first find the <p> element, then verify its content
            counterComponent.Find("p").MarkupMatches("<p>Current count: 2</p>");

            counterComponent.Instance.Start.Should().Be(2);
        }
    }
}