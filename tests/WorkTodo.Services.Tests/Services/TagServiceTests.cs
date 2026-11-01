using WorkTodo.Services.Exceptions;
using WorkTodo.Services.Tests.Fakes;

namespace WorkTodo.Services.Tests.Services;

public class TagServiceTests
{
    private readonly FakeTagRepository _tags = new();
    private readonly TagService _service;

    public TagServiceTests()
    {
        _service = new TagService(_tags);
    }

    [Fact]
    public async Task All_tags_are_returned_ordered_by_name()
    {
        _tags.Seed(TestData.CreateTag(1, "home"), TestData.CreateTag(2, "admin"));

        var tags = await _service.GetAllAsync();

        Assert.Equal(["admin", "home"], tags.Select(tag => tag.Name));
    }

    [Fact]
    public async Task Missing_tag_is_created()
    {
        var tag = await _service.GetOrCreateAsync("work");

        Assert.NotEqual(0, tag.Id);
        Assert.Contains(tag, _tags.Tags);
    }

    [Fact]
    public async Task Existing_tag_is_reused_regardless_of_case_and_surrounding_spaces()
    {
        var existing = TestData.CreateTag(1, "work");
        _tags.Seed(existing);

        var tag = await _service.GetOrCreateAsync("  WORK ");

        Assert.Same(existing, tag);
        Assert.Single(_tags.Tags);
    }

    [Fact]
    public async Task Blank_tag_name_is_rejected()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _service.GetOrCreateAsync("  "));

        Assert.Empty(_tags.Tags);
    }

    [Fact]
    public async Task Deleted_tag_is_removed()
    {
        _tags.Seed(TestData.CreateTag(1, "work"));

        await _service.DeleteAsync(1);

        Assert.Empty(_tags.Tags);
    }

    [Fact]
    public async Task Deleting_an_unknown_tag_throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(42));
    }
}
