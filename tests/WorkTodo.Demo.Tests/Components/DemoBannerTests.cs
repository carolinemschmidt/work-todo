using Bunit;
using WorkTodo.Demo.Components;

namespace WorkTodo.Demo.Tests.Components;

public class DemoBannerTests : BunitContext
{
    [Fact]
    public void Banner_tells_visitors_that_the_data_resets_on_reload()
    {
        var cut = Render<DemoBanner>();

        var banner = cut.Find("[role='note']");
        Assert.Contains("demo", banner.TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reload", banner.TextContent, StringComparison.OrdinalIgnoreCase);
    }
}
