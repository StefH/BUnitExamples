using BlazorApp.WebAssembly.Components;
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
            counterComponent.Find("button").Click();

            // Assert: first find the <p> element, then verify its content
            counterComponent.Find("p").MarkupMatches("<p>Current count: 2</p>");

            // Assert: check if the start value is now 2
            counterComponent.Instance.Start.Should().Be(2);
        }

        [Fact]
        public void CounterShouldUseStartFluentInterface()
        {
            // Arrange / Act: render the SpecialCounter.razor component
            var counterComponent = RenderComponent<SpecialCounter>(componentParameterBuilder =>
            {
                componentParameterBuilder.Add(specialCounter => specialCounter.Start, 3);
            });

            // Assert: first find the <p> element, then verify its content
            counterComponent.Find("p").MarkupMatches("<p>Current count: 3</p>");
        }
    }
}