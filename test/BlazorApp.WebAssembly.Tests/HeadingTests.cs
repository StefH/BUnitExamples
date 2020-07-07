using BlazorApp.WebAssembly.Components;
using Bunit;
using Xunit;

namespace BlazorApp.WebAssembly.Tests
{
    public class HeadingTests : TestContext
    {
        [Fact]
        public void MarkupMatches()
        {
            // Arrange / Act: render the Heading.razor component
            var headingComponent = RenderComponent<Heading>();

            // Assert: use the MarkupMatches() method to perform a semantic comparison of the output of the <Heading> component,
            headingComponent.MarkupMatches(@"<h3 id=""heading-1337"" required>
                      Heading text
                      <small class=""mark text-muted"">Secondary text</small>
                    </h3>");
        }
    }
}