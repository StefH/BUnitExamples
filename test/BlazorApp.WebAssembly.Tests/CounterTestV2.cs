using BlazorApp.WebAssembly.Pages;
using Bunit;
using Xunit;

namespace BlazorApp.WebAssembly.Tests
{
    public class CounterTestV2 : TestContext
    {
        [Fact]
        public void CounterShouldIncrementWhenClicked()
        {
            // Arrange: render the Counter.razor component
            var counterComponent = RenderComponent<Counter>();

            // Act: find and click the <button> element to increment
            // the counter in the <p> element
            counterComponent.Find("button").Click();

            // Assert: first find the <p> element, then verify its content
            counterComponent.Find("p").MarkupMatches("<p>Current count: 1</p>");
        }
    }
}