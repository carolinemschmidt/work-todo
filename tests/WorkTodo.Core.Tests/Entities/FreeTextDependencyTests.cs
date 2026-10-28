using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;

namespace WorkTodo.Core.Tests.Entities;

public class FreeTextDependencyTests
{
    [Fact]
    public void New_dependency_is_unresolved()
    {
        var dependency = new FreeTextDependency("Waiting for finance");

        Assert.False(dependency.IsResolved);
    }

    [Fact]
    public void Resolve_ticks_the_dependency_off()
    {
        var dependency = new FreeTextDependency("Waiting for finance");

        dependency.Resolve();

        Assert.True(dependency.IsResolved);
    }

    [Fact]
    public void Reopen_unticks_a_resolved_dependency()
    {
        var dependency = new FreeTextDependency("Waiting for finance");
        dependency.Resolve();

        dependency.Reopen();

        Assert.False(dependency.IsResolved);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_text_is_rejected(string? text)
    {
        Assert.ThrowsAny<ArgumentException>(() => new FreeTextDependency(text!));
    }

    [Fact]
    public void Too_long_text_is_rejected()
    {
        var text = new string('a', ValidationConstants.FreeTextDependencyMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new FreeTextDependency(text));
    }
}
