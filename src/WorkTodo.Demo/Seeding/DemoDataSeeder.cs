using WorkTodo.Core.Enums;
using WorkTodo.Services;

namespace WorkTodo.Demo.Seeding;

public class DemoDataSeeder(ITodoService todos, IDependencyService dependencies, TimeProvider time)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);
        var idsByTitle = new Dictionary<string, int>();

        foreach (var spec in DemoDataSet.Items)
        {
            idsByTitle[spec.Title] = await CreateItemAsync(spec, today, cancellationToken);
        }

        foreach (var spec in DemoDataSet.Items)
        {
            await AddDependenciesAsync(idsByTitle[spec.Title], spec, idsByTitle, cancellationToken);
        }
    }

    private async Task<int> CreateItemAsync(DemoItemSpec spec, DateOnly today, CancellationToken cancellationToken)
    {
        var dueDate = spec.DueInDays is { } days ? today.AddDays(days) : (DateOnly?)null;
        var item = await todos.CreateAsync(spec.Title, spec.Priority, spec.Description, dueDate, cancellationToken);

        if (spec.Status != TodoStatus.NotStarted)
        {
            await todos.ChangeStatusAsync(item.Id, spec.Status, cancellationToken);
        }

        foreach (var tag in spec.Tags)
        {
            await todos.AddTagAsync(item.Id, tag, cancellationToken);
        }

        foreach (var key in spec.TicketKeys)
        {
            await todos.AddTicketLinkAsync(item.Id, DemoSeedConstants.TicketBaseUrl + key, key, cancellationToken);
        }

        return item.Id;
    }

    private async Task AddDependenciesAsync(
        int itemId,
        DemoItemSpec spec,
        IReadOnlyDictionary<string, int> idsByTitle,
        CancellationToken cancellationToken)
    {
        foreach (var title in spec.DependsOnTitles)
        {
            await dependencies.AddDependencyAsync(itemId, idsByTitle[title], cancellationToken);
        }

        foreach (var freeText in spec.FreeTextDependencies)
        {
            await AddFreeTextDependencyAsync(itemId, freeText, cancellationToken);
        }
    }

    private async Task AddFreeTextDependencyAsync(int itemId, DemoFreeTextSpec spec, CancellationToken cancellationToken)
    {
        await dependencies.AddFreeTextDependencyAsync(itemId, spec.Text, cancellationToken);
        if (!spec.IsResolved)
        {
            return;
        }

        var item = await todos.GetAsync(itemId, cancellationToken);
        var added = item.FreeTextDependencies.Single(dependency => dependency.Text == spec.Text);
        await dependencies.ResolveFreeTextDependencyAsync(itemId, added.Id, cancellationToken);
    }
}
