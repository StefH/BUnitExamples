using BlazorApp.WebAssembly.Pages;
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorApp.WebAssembly.Tests
{
    public class CounterTest
    {
        [Fact]
        public void CounterShouldIncrementWhenClicked()
        {
            using var ctx = new TestContext();

            // Arrange: render the Counter.razor component
            var counterComponent = ctx.RenderComponent<Counter>();

            // Assert: verifying the initial state of a Component
            counterComponent.Instance.CurrentCount.Should().Be(0);

            // Act: find and click the <button> element to increment
            // the counter in the <p> element
            counterComponent.Find("button").Click();

            // Assert: first find the <p> element, then verify its content
            counterComponent.Find("p").MarkupMatches("<p>Current count: 1</p>");

            // Assert: verifying the final state of a Component
            counterComponent.Instance.CurrentCount.Should().Be(1);
        }
    }
}