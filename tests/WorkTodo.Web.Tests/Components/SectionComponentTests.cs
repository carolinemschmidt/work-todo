using Bunit;
using Microsoft.AspNetCore.Components;
using WorkTodo.Core.Entities;
using WorkTodo.Core.Enums;
using WorkTodo.Web.Components.Shared;

namespace WorkTodo.Web.Tests.Components;

public class SectionComponentTests : BunitContext
{
    [Fact]
    public void Tag_section_shows_the_tags_and_raises_remove_with_the_tag()
    {
        var work = new Tag("work");
        Tag? removed = null;
        var cut = Render<TagSection>(p => p
            .Add(c => c.Tags, [work])
            .Add(c => c.OnRemove, EventCallback.Factory.Create<Tag>(this, tag => removed = tag)));

        cut.Find("button[aria-label='Remove tag work']").Click();

        Assert.Same(work, removed);
    }

    [Fact]
    public void Tag_section_raises_the_add_callback_with_the_typed_name()
    {
        string? added = null;
        Func<string, Task<bool>> onAdd = name =>
        {
            added = name;
            return Task.FromResult(true);
        };
        var cut = Render<TagSection>(p => p.Add(c => c.Tags, []).Add(c => c.OnAdd, onAdd));

        cut.Find("input[aria-label='New tag']").Input("urgent");
        cut.Find("form").Submit();

        Assert.Equal("urgent", added);
    }

    [Fact]
    public void Tag_section_clears_the_name_after_a_successful_add()
    {
        Func<string, Task<bool>> onAdd = _ => Task.FromResult(true);
        var cut = Render<TagSection>(p => p.Add(c => c.Tags, []).Add(c => c.OnAdd, onAdd));
        cut.Find("input[aria-label='New tag']").Input("urgent");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
            Assert.Equal(string.Empty, cut.Find("input[aria-label='New tag']").GetAttribute("value")));
    }

    [Fact]
    public void Tag_section_keeps_the_name_when_the_add_fails()
    {
        Func<string, Task<bool>> onAdd = _ => Task.FromResult(false);
        var cut = Render<TagSection>(p => p.Add(c => c.Tags, []).Add(c => c.OnAdd, onAdd));
        cut.Find("input[aria-label='New tag']").Input("urgent");

        cut.Find("form").Submit();

        Assert.Equal("urgent", cut.Find("input[aria-label='New tag']").GetAttribute("value"));
    }

    [Fact]
    public void Ticket_link_section_shows_the_label_or_falls_back_to_the_url()
    {
        var labelled = new TicketLink("https://tickets.example.com/ABC-1", "ABC-1");
        var unlabelled = new TicketLink("https://tickets.example.com/ABC-2");

        var cut = Render<TicketLinkSection>(p => p.Add(c => c.Links, [labelled, unlabelled]));

        var texts = cut.FindAll("a.ticket-link").Select(link => link.TextContent);
        Assert.Equal(["ABC-1", "https://tickets.example.com/ABC-2"], texts);
    }

    [Fact]
    public void Ticket_link_section_raises_remove_with_the_link()
    {
        var link = new TicketLink("https://tickets.example.com/ABC-1", "ABC-1");
        TicketLink? removed = null;
        var cut = Render<TicketLinkSection>(p => p
            .Add(c => c.Links, [link])
            .Add(c => c.OnRemove, EventCallback.Factory.Create<TicketLink>(this, value => removed = value)));

        cut.Find("button[aria-label='Remove link ABC-1']").Click();

        Assert.Same(link, removed);
    }

    [Fact]
    public void Ticket_link_section_raises_the_add_callback_with_url_and_label()
    {
        (string Url, string Label)? added = null;
        Func<string, string, Task<bool>> onAdd = (url, label) =>
        {
            added = (url, label);
            return Task.FromResult(true);
        };
        var cut = Render<TicketLinkSection>(p => p.Add(c => c.Links, []).Add(c => c.OnAdd, onAdd));

        cut.Find("input[aria-label='Ticket URL']").Input("https://tickets.example.com/ABC-1");
        cut.Find("input[aria-label='Ticket label']").Input("ABC-1");
        cut.Find("form").Submit();

        Assert.Equal(("https://tickets.example.com/ABC-1", "ABC-1"), added);
    }

    [Fact]
    public void Ticket_link_section_keeps_the_input_when_the_add_fails()
    {
        Func<string, string, Task<bool>> onAdd = (_, _) => Task.FromResult(false);
        var cut = Render<TicketLinkSection>(p => p.Add(c => c.Links, []).Add(c => c.OnAdd, onAdd));
        cut.Find("input[aria-label='Ticket URL']").Input("not a url");

        cut.Find("form").Submit();

        Assert.Equal("not a url", cut.Find("input[aria-label='Ticket URL']").GetAttribute("value"));
    }

    [Fact]
    public void Dependency_section_lists_dependencies_and_raises_remove_with_the_item()
    {
        var other = Item("Write spec");
        TodoItem? removed = null;
        var cut = Render<DependencySection>(p => p
            .Add(c => c.DependsOn, [other])
            .Add(c => c.Candidates, [])
            .Add(c => c.OnRemove, EventCallback.Factory.Create<TodoItem>(this, value => removed = value)));

        cut.Find("button[aria-label='Remove dependency on Write spec']").Click();

        Assert.Same(other, removed);
    }

    [Fact]
    public void Dependency_section_offers_the_candidates_after_the_placeholder()
    {
        var cut = Render<DependencySection>(p => p
            .Add(c => c.DependsOn, [])
            .Add(c => c.Candidates, [Item("First"), Item("Second")]));

        var options = cut.FindAll("select option").Select(option => option.TextContent);

        Assert.Equal(["Choose a todo…", "First", "Second"], options);
    }

    [Fact]
    public void Dependency_section_raises_the_add_callback_with_the_chosen_id()
    {
        int? added = null;
        Func<int, Task> onAdd = id =>
        {
            added = id;
            return Task.CompletedTask;
        };
        var candidate = Item("Write spec", id: 7);
        var cut = Render<DependencySection>(p => p
            .Add(c => c.DependsOn, [])
            .Add(c => c.Candidates, [candidate])
            .Add(c => c.OnAdd, onAdd));

        cut.Find("select[aria-label='Depends on']").Change("7");
        cut.Find("form").Submit();

        Assert.Equal(7, added);
    }

    [Fact]
    public void Dependency_section_does_nothing_while_no_todo_is_chosen()
    {
        var called = false;
        Func<int, Task> onAdd = _ =>
        {
            called = true;
            return Task.CompletedTask;
        };
        var cut = Render<DependencySection>(p => p
            .Add(c => c.DependsOn, [])
            .Add(c => c.Candidates, [Item("Write spec", id: 7)])
            .Add(c => c.OnAdd, onAdd));

        cut.Find("form").Submit();

        Assert.False(called);
    }

    private static TodoItem Item(string title, int id = 1)
    {
        var item = new TodoItem(title, Priority.Medium);
        typeof(TodoItem).GetProperty(nameof(TodoItem.Id))!.SetValue(item, id);
        return item;
    }
}
