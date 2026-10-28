using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;

namespace WorkTodo.Core.Tests.Entities;

public class TagTests
{
    [Fact]
    public void Name_is_trimmed()
    {
        var tag = new Tag("  work  ");

        Assert.Equal("work", tag.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_name_is_rejected(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Tag(name!));
    }

    [Fact]
    public void Too_long_name_is_rejected()
    {
        var name = new string('a', ValidationConstants.TagNameMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new Tag(name));
    }
}
