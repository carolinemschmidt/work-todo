using Bunit;
using Microsoft.AspNetCore.Components;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Web.Components.Shared;

namespace WorkTodo.Web.Tests.Components;

public class SharedComponentTests : BunitContext
{
    [Fact]
    public void Tag_badge_shows_the_name_without_a_remove_button_by_default()
    {
        var cut = Render<TagBadge>(p => p.Add(c => c.Name, "work"));

        Assert.Contains("work", cut.Markup);
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void Tag_badge_remove_button_raises_the_callback()
    {
        var removed = false;
        var cut = Render<TagBadge>(p => p
            .Add(c => c.Name, "work")
            .Add(c => c.OnRemove, EventCallback.Factory.Create(this, () => removed = true)));

        cut.Find("button[aria-label='Remove tag work']").Click();

        Assert.True(removed);
    }

    [Fact]
    public void Enum_select_lists_every_value_and_the_any_option()
    {
        var cut = Render<EnumSelect<Priority>>(p => p
            .Add(c => c.Label, "Priority")
            .Add(c => c.AnyText, "All priorities"));

        var options = cut.FindAll("option").Select(option => option.TextContent);

        Assert.Equal(["All priorities", "Low", "Medium", "High", "Critical"], options);
    }

    [Fact]
    public void Enum_select_without_any_text_has_no_empty_option()
    {
        var cut = Render<EnumSelect<Priority>>(p => p.Add(c => c.Label, "Priority"));

        Assert.Equal(4, cut.FindAll("option").Count);
    }

    [Fact]
    public void Enum_select_reports_the_chosen_value()
    {
        Priority? chosen = null;
        var cut = Render<EnumSelect<Priority>>(p => p
            .Add(c => c.Label, "Priority")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<Priority?>(this, value => chosen = value)));

        cut.Find("select").Change("High");

        Assert.Equal(Priority.High, chosen);
    }

    [Fact]
    public void Enum_select_reports_null_when_the_any_option_is_chosen()
    {
        Priority? chosen = Priority.High;
        var cut = Render<EnumSelect<Priority>>(p => p
            .Add(c => c.Label, "Priority")
            .Add(c => c.AnyText, "All priorities")
            .Add(c => c.Value, Priority.High)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<Priority?>(this, value => chosen = value)));

        cut.Find("select").Change(string.Empty);

        Assert.Null(chosen);
    }

    [Fact]
    public void Enum_select_marks_the_current_value_as_selected()
    {
        var cut = Render<EnumSelect<TodoStatus>>(p => p
            .Add(c => c.Label, "Status")
            .Add(c => c.Value, TodoStatus.InProgress));

        Assert.Equal("In progress", cut.Find("option[selected]").TextContent);
    }

    [Fact]
    public void Free_text_dependency_list_shows_resolved_state()
    {
        var resolved = new FreeTextDependency("Done thing");
        resolved.Resolve();
        var open = new FreeTextDependency("Open thing");

        var cut = Render<FreeTextDependencyList>(p => p.Add(c => c.Dependencies, [resolved, open]));

        Assert.True(cut.Find("input[aria-label='Resolved: Done thing']").HasAttribute("checked"));
        Assert.False(cut.Find("input[aria-label='Resolved: Open thing']").HasAttribute("checked"));
    }

    [Fact]
    public void Free_text_dependency_list_raises_toggle_and_remove_callbacks()
    {
        var dependency = new FreeTextDependency("Waiting for finance");
        FreeTextDependency? toggled = null;
        FreeTextDependency? removed = null;
        var cut = Render<FreeTextDependencyList>(p => p
            .Add(c => c.Dependencies, [dependency])
            .Add(c => c.OnToggle, EventCallback.Factory.Create<FreeTextDependency>(this, value => toggled = value))
            .Add(c => c.OnRemove, EventCallback.Factory.Create<FreeTextDependency>(this, value => removed = value)));

        cut.Find("input[aria-label='Resolved: Waiting for finance']").Change(true);
        cut.Find("button[aria-label='Remove free-text dependency Waiting for finance']").Click();

        Assert.Same(dependency, toggled);
        Assert.Same(dependency, removed);
    }

    [Fact]
    public void Free_text_dependency_list_raises_the_add_callback_with_the_typed_text()
    {
        string? added = null;
        var cut = Render<FreeTextDependencyList>(p => p
            .Add(c => c.Dependencies, [])
            .Add(c => c.OnAdd, EventCallback.Factory.Create<string>(this, text => added = text)));

        cut.Find("input[aria-label='New free-text dependency']").Input("Waiting for finance");
        cut.Find("form").Submit();

        Assert.Equal("Waiting for finance", added);
    }

    [Fact]
    public void Free_text_dependency_list_ignores_blank_text()
    {
        var called = false;
        var cut = Render<FreeTextDependencyList>(p => p
            .Add(c => c.Dependencies, [])
            .Add(c => c.OnAdd, EventCallback.Factory.Create<string>(this, _ => called = true)));

        cut.Find("input[aria-label='New free-text dependency']").Input("   ");
        cut.Find("form").Submit();

        Assert.False(called);
    }
}
