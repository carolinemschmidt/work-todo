using WorkTodo.Core.Enums;

namespace WorkTodo.Demo.Seeding;

public sealed record DemoItemSpec(
    string Title,
    Priority Priority,
    string? Description,
    int? DueInDays,
    TodoStatus Status,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> TicketKeys,
    IReadOnlyList<DemoFreeTextSpec> FreeTextDependencies,
    IReadOnlyList<string> DependsOnTitles);

public sealed record DemoFreeTextSpec(string Text, bool IsResolved);
