using Bunit;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Entities;
using WorkTodo.Data.Constants;
using WorkTodo.Data.Exceptions;
using WorkTodo.Services;
using WorkTodo.UI.Components.Pages;
using WorkTodo.UI.Constants;

namespace WorkTodo.UI.Tests.Pages;

public class DatabaseFailureTests : WebTestBase
{
    [Fact]
    public void Failed_save_shows_a_generic_message_without_internal_details()
    {
        Services.AddScoped<ITagService>(_ => new FailingTagService(
            new DataAccessException("internal detail", new InvalidOperationException())));
        var cut = Render<TagsPage>();

        cut.Find("input[aria-label='New tag name']").Input("work");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains(UiConstants.DatabaseErrorMessage, cut.Find(".alert-danger").TextContent);
            Assert.DoesNotContain("internal detail", cut.Markup);
        });
    }

    [Fact]
    public void Database_error_while_loading_shows_a_generic_message()
    {
        Services.AddScoped<ITagService>(_ => new FailingTagService(new SqliteException("connection lost", 1)));

        var cut = Render<TagsPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains(UiConstants.DatabaseErrorMessage, cut.Find(".alert-danger").TextContent);
            Assert.DoesNotContain("connection lost", cut.Markup);
        });
    }

    [Fact]
    public void A_duplicate_says_it_already_exists_instead_of_blaming_the_database()
    {
        Services.AddScoped<ITagService>(_ => new FailingTagService(
            new DuplicateEntryException(new InvalidOperationException("internal detail"))));
        var cut = Render<TagsPage>();

        cut.Find("input[aria-label='New tag name']").Input("work");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            var message = cut.Find(".alert-danger").TextContent;
            Assert.Contains(DatabaseConstants.DuplicateEntryMessage, message);
            Assert.DoesNotContain(UiConstants.DatabaseErrorMessage, message);
            Assert.DoesNotContain("internal detail", cut.Markup);
        });
    }

    [Fact]
    public void Failed_load_on_the_tags_page_shows_the_error_with_a_retry_button_and_no_spinner()
    {
        AssertLoadFailureIsShownWithRetry<TagsPage>();
    }

    [Fact]
    public void Failed_load_on_the_todo_list_shows_the_error_with_a_retry_button_and_no_spinner()
    {
        AssertLoadFailureIsShownWithRetry<TodoListPage>();
    }

    [Fact]
    public void Failed_load_on_the_next_up_page_shows_the_error_with_a_retry_button_and_no_spinner()
    {
        AssertLoadFailureIsShownWithRetry<NextUpPage>();
    }

    [Fact]
    public void Try_again_reloads_the_page_once_the_database_is_back()
    {
        var tagService = new RecoveringTagService();
        Services.AddScoped<ITagService>(_ => tagService);
        var cut = Render<TagsPage>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("button.retry-button")));

        cut.Find("button.retry-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll(".alert-danger"));
            Assert.Empty(cut.FindAll("button.retry-button"));
            Assert.Contains("work", cut.Find(".tag-list").TextContent);
        });
    }

    private void AssertLoadFailureIsShownWithRetry<TPage>()
        where TPage : Microsoft.AspNetCore.Components.IComponent
    {
        Services.AddScoped<ITagService>(_ => new FailingTagService(new SqliteException("connection lost", 1)));
        Services.AddScoped<INextUpService>(_ => new FailingNextUpService());

        var cut = Render<TPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains(UiConstants.DatabaseErrorMessage, cut.Find(".alert-danger").TextContent);
            Assert.Empty(cut.FindAll("[role='status']"));
            Assert.NotEmpty(cut.FindAll("button.retry-button"));
        });
    }

    private sealed class RecoveringTagService : ITagService
    {
        private int _loads;

        public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _loads++ == 0
                ? Task.FromException<IReadOnlyList<Tag>>(new SqliteException("connection lost", 1))
                : Task.FromResult<IReadOnlyList<Tag>>([new Tag("work")]);
        }

        public Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Tag(name));
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FailingNextUpService : INextUpService
    {
        public Task<IReadOnlyList<TodoItem>> GetNextUpAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IReadOnlyList<TodoItem>>(new SqliteException("connection lost", 1));
        }

        public Task<IReadOnlySet<int>> GetBlockedIdsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IReadOnlySet<int>>(new SqliteException("connection lost", 1));
        }
    }

    private sealed class FailingTagService(Exception failure) : ITagService
    {
        public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return failure is DataAccessException
                ? Task.FromResult<IReadOnlyList<Tag>>([])
                : Task.FromException<IReadOnlyList<Tag>>(failure);
        }

        public Task<Tag> GetOrCreateAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromException<Tag>(failure);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromException(failure);
        }
    }
}
