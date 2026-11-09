using WorkTodo.Core.Enums;

namespace WorkTodo.Demo.Seeding;

public static class DemoDataSet
{
    public static IReadOnlyList<DemoItemSpec> Items { get; } =
    [
        new DemoItemSpec(
            "Collect sales numbers",
            Priority.Medium,
            "Export the regional figures from the sales system.",
            DueInDays: 1,
            TodoStatus.InProgress,
            Tags: ["finance"],
            TicketKeys: ["FIN-101"],
            FreeTextDependencies: [],
            DependsOnTitles: []),
        new DemoItemSpec(
            "Write quarterly report",
            Priority.High,
            "Summarise revenue, costs and the outlook for next quarter.",
            DueInDays: 4,
            TodoStatus.NotStarted,
            Tags: ["work", "finance"],
            TicketKeys: [],
            FreeTextDependencies: [new DemoFreeTextSpec("Approval from the controller", IsResolved: false)],
            DependsOnTitles: ["Collect sales numbers"]),
        new DemoItemSpec(
            "Fix login bug",
            Priority.Critical,
            "Users are logged out after ten minutes instead of thirty.",
            DueInDays: 1,
            TodoStatus.InProgress,
            Tags: ["bug", "work"],
            TicketKeys: ["ABC-42"],
            FreeTextDependencies: [],
            DependsOnTitles: []),
        new DemoItemSpec(
            "Review pull request",
            Priority.Medium,
            "Check the new export feature before it is merged.",
            DueInDays: 2,
            TodoStatus.NotStarted,
            Tags: ["work"],
            TicketKeys: [],
            FreeTextDependencies: [new DemoFreeTextSpec("Author pushes the requested changes", IsResolved: true)],
            DependsOnTitles: []),
        new DemoItemSpec(
            "Book team offsite",
            Priority.Low,
            Description: null,
            DueInDays: null,
            TodoStatus.NotStarted,
            Tags: ["planning"],
            TicketKeys: [],
            FreeTextDependencies: [],
            DependsOnTitles: []),
        new DemoItemSpec(
            "Update onboarding docs",
            Priority.Low,
            "Add the new VPN setup steps.",
            DueInDays: 14,
            TodoStatus.Done,
            Tags: ["docs", "work"],
            TicketKeys: [],
            FreeTextDependencies: [],
            DependsOnTitles: []),
    ];
}
