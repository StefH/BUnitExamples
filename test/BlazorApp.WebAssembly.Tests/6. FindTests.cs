using BlazorApp.WebAssembly.Components;
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorApp.WebAssembly.Tests
{
    public class FindTests : TestContext
    {
        [Fact]
        public void FindAndFindAll()
        {
            // Arrange / Act: render the FancyTable.razor component
            var headingComponent = RenderComponent<FancyTable>();

            // Assert: Once you have one or more elements, you verify against them by e.g. inspecting their properties through the DOM API.
            var tableCaption = headingComponent.Find("caption");
            tableCaption.Attributes.Should().BeEmpty();

            var tableCells = headingComponent.FindAll("td:first-child");
            tableCells.Should().HaveCount(2);
            Assert.All(tableCells, td => td.HasAttribute("style"));
        }
    }
}