using BlazorApp.WebAssembly.Components;
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorApp.WebAssembly.Tests
{
    public class CounterDarkTests : TestContext
    {
        [Fact]
        public void CounterDark_Should_Use_UnnamedCascadingParameter_IsDark_True()
        {
            // Arrange / Act: render the CounterDark.razor component
            var counterComponent = RenderComponent<CounterDark>(componentParameterBuilder =>
            {
                componentParameterBuilder.Add(true);
            });

            // Assert: first find the <div> element, then verify its style
            counterComponent.Find("div").Attributes.GetNamedItem("style").Value.Should().Contain("darkgrey");
        }

        [Fact]
        public void CounterDark_Should_Use_UnnamedCascadingParameter_IsDark_False()
        {
            // Arrange / Act: render the CounterDark.razor component
            var counterComponent = RenderComponent<CounterDark>(componentParameterBuilder =>
            {
                componentParameterBuilder.Add(false);
            });

            // Assert: first find the <div> element, then verify its style
            counterComponent.Find("div").Attributes.GetNamedItem("style").Value.Should().Contain("white");
        }
    }
}
