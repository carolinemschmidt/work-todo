using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.UI.Components;

namespace WorkTodo.UI.Tests.Components;

public class ScopedPageBaseTests : BunitContext
{
    public sealed class ScopeProbe : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }

    public sealed class ProbingPage : ScopedPageBase
    {
        public ScopeProbe FirstProbe => ScopedServices.GetRequiredService<ScopeProbe>();

        public ScopeProbe SecondProbe => ScopedServices.GetRequiredService<ScopeProbe>();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, "page");
        }
    }

    public ScopedPageBaseTests()
    {
        Services.AddScoped<ScopeProbe>();
    }

    [Fact]
    public void Services_requested_by_one_page_share_a_scope()
    {
        var page = Render<ProbingPage>().Instance;

        Assert.Same(page.FirstProbe, page.SecondProbe);
    }

    [Fact]
    public void Each_page_gets_its_own_scope()
    {
        var first = Render<ProbingPage>().Instance;
        var second = Render<ProbingPage>().Instance;

        Assert.NotSame(first.FirstProbe, second.FirstProbe);
    }

    [Fact]
    public void Disposing_the_page_disposes_its_scope()
    {
        var cut = Render<ProbingPage>();
        var probe = cut.Instance.FirstProbe;

        ((IDisposable)cut.Instance).Dispose();

        Assert.True(probe.IsDisposed);
    }

    [Fact]
    public void Scope_is_not_created_again_after_the_page_is_disposed()
    {
        var page = Render<ProbingPage>().Instance;
        ((IDisposable)page).Dispose();

        Assert.Throws<ObjectDisposedException>(() => page.FirstProbe);
    }
}
