using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WorkTodo.Web.Components.Shared;

namespace WorkTodo.Web.Tests.Components;

public class AppErrorBoundaryTests : BunitContext
{
    public sealed class SometimesFailingComponent : ComponentBase
    {
        public static bool ShouldFail { get; set; }

        protected override void OnInitialized()
        {
            if (ShouldFail)
            {
                throw new InvalidOperationException("boom");
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, "child content");
        }
    }

    [Fact]
    public void Child_content_is_shown_when_nothing_fails()
    {
        SometimesFailingComponent.ShouldFail = false;

        var cut = Render<AppErrorBoundary>(p => p.AddChildContent<SometimesFailingComponent>());

        Assert.Contains("child content", cut.Markup);
        Assert.Empty(cut.FindAll(".alert-danger"));
    }

    [Fact]
    public void Friendly_message_is_shown_when_a_child_fails()
    {
        SometimesFailingComponent.ShouldFail = true;

        var cut = Render<AppErrorBoundary>(p => p.AddChildContent<SometimesFailingComponent>());

        Assert.Contains("Something went wrong", cut.Find(".alert-danger").TextContent);
        Assert.DoesNotContain("boom", cut.Markup);
    }

    [Fact]
    public void Try_again_shows_the_child_content_once_it_works()
    {
        SometimesFailingComponent.ShouldFail = true;
        var cut = Render<AppErrorBoundary>(p => p.AddChildContent<SometimesFailingComponent>());
        SometimesFailingComponent.ShouldFail = false;

        cut.Find("button").Click();

        cut.WaitForAssertion(() => Assert.Contains("child content", cut.Markup));
        Assert.Empty(cut.FindAll(".alert-danger"));
    }
}
