using WorkTodo.Core.Entities;
using WorkTodo.Services.Exceptions;
using WorkTodo.Services.Tests.Fakes;

namespace WorkTodo.Services.Tests.Services;

public class NotFoundMessageTests
{
    private readonly FakeTodoRepository _todos = new();
    private readonly FakeTagRepository _tags = new();
    private readonly FakeTodoDependencyRepository _dependencies = new();

    [Fact]
    public async Task Unknown_todo_is_described_in_plain_words()
    {
        var service = new TodoService(_todos, new TagService(_tags));

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(42));

        Assert.Equal("Todo 42 was not found. It may have been deleted.", exception.Message);
    }

    [Fact]
    public async Task Unknown_tag_is_described_in_plain_words()
    {
        var service = new TagService(_tags);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(42));

        Assert.Equal("Tag 42 was not found. It may have been deleted.", exception.Message);
    }

    [Fact]
    public async Task Unknown_ticket_link_is_described_in_plain_words()
    {
        _todos.Seed(TestData.CreateItem(1));
        var service = new TodoService(_todos, new TagService(_tags));

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.RemoveTicketLinkAsync(1, 99));

        Assert.Equal("Ticket link 99 was not found. It may have been deleted.", exception.Message);
    }

    [Fact]
    public async Task Unknown_free_text_dependency_is_described_in_plain_words()
    {
        _todos.Seed(TestData.CreateItem(1));
        var service = new DependencyService(_todos, _dependencies);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.ResolveFreeTextDependencyAsync(1, 99));

        Assert.Equal("Dependency 99 was not found. It may have been deleted.", exception.Message);
    }
}
