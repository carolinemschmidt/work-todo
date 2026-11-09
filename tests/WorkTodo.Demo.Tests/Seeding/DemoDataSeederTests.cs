using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Demo.Seeding;
using WorkTodo.Services;

namespace WorkTodo.Demo.Tests.Seeding;

public class DemoDataSeederTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    private readonly ServiceProvider _provider;

    public DemoDataSeederTests()
    {
        var services = new ServiceCollection().AddDemo();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2026, 11, 9, 8, 0, 0, TimeSpan.Zero)));
        _provider = services.BuildServiceProvider(validateScopes: true);
    }

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task Seeded_items_cover_every_priority_and_status()
    {
        var items = await SeedAndLoadAsync();

        Assert.Equal(Enum.GetValues<Priority>().Order(), items.Select(item => item.Priority).Distinct().Order());
        Assert.Equal(Enum.GetValues<TodoStatus>().Order(), items.Select(item => item.Status).Distinct().Order());
    }

    [Fact]
    public async Task Open_items_with_a_due_date_are_due_within_the_next_few_weeks()
    {
        var items = await SeedAndLoadAsync();

        var dueDates = items.Where(item => item.Status != TodoStatus.Done && item.DueDate is not null)
            .Select(item => item.DueDate!.Value)
            .ToList();
        Assert.NotEmpty(dueDates);
        Assert.All(dueDates, dueDate => Assert.InRange(dueDate, Today, Today.AddDays(30)));
    }

    [Fact]
    public async Task Exactly_one_item_is_blocked_by_another_unfinished_item()
    {
        await SeedAndLoadAsync();

        using var scope = _provider.CreateScope();
        var blocked = await scope.ServiceProvider.GetRequiredService<INextUpService>().GetBlockedIdsAsync();

        Assert.Single(blocked);
    }

    [Fact]
    public async Task Next_up_leaves_out_blocked_and_finished_items()
    {
        await SeedAndLoadAsync();

        using var scope = _provider.CreateScope();
        var blocked = await scope.ServiceProvider.GetRequiredService<INextUpService>().GetBlockedIdsAsync();
        var nextUp = await scope.ServiceProvider.GetRequiredService<INextUpService>().GetNextUpAsync();

        Assert.NotEmpty(nextUp);
        Assert.DoesNotContain(nextUp, item => blocked.Contains(item.Id));
        Assert.DoesNotContain(nextUp, item => item.Status == TodoStatus.Done);
    }

    [Fact]
    public async Task Free_text_dependencies_include_one_that_is_resolved_and_one_that_is_open()
    {
        var items = await SeedAndLoadAsync();

        var dependencies = items.SelectMany(item => item.FreeTextDependencies).ToList();
        Assert.Contains(dependencies, dependency => dependency.IsResolved);
        Assert.Contains(dependencies, dependency => !dependency.IsResolved);
    }

    [Fact]
    public async Task At_least_one_tag_is_shared_between_items_and_one_item_has_a_ticket_link()
    {
        var items = await SeedAndLoadAsync();

        var sharedTags = items.SelectMany(item => item.Tags).GroupBy(tag => tag.Id).Where(group => group.Count() > 1);
        Assert.NotEmpty(sharedTags);
        Assert.Contains(items, item => item.TicketLinks.Count > 0);
    }

    private async Task<IReadOnlyList<TodoItem>> SeedAndLoadAsync()
    {
        await _provider.SeedDemoDataAsync();

        using var scope = _provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ITodoService>().GetAllAsync();
    }
}
