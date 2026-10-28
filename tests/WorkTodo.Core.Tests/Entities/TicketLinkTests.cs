using WorkTodo.Core.Constants;
using WorkTodo.Core.Entities;

namespace WorkTodo.Core.Tests.Entities;

public class TicketLinkTests
{
    [Fact]
    public void Label_is_optional()
    {
        var link = new TicketLink("https://tickets.example.com/ABC-1");

        Assert.Null(link.Label);
    }

    [Theory]
    [InlineData("https://tickets.example.com/ABC-1")]
    [InlineData("http://tickets.example.com/ABC-1")]
    public void Http_and_https_urls_are_accepted(string url)
    {
        var link = new TicketLink(url);

        Assert.Equal(url, link.Url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a url")]
    [InlineData("/relative/path")]
    [InlineData("ftp://tickets.example.com/ABC-1")]
    public void Invalid_url_is_rejected(string? url)
    {
        Assert.ThrowsAny<ArgumentException>(() => new TicketLink(url!));
    }

    [Fact]
    public void Too_long_label_is_rejected()
    {
        var label = new string('a', ValidationConstants.TicketLabelMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new TicketLink("https://tickets.example.com/ABC-1", label));
    }
}
