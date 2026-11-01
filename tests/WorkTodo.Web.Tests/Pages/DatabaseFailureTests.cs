using Bunit;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Core.Entities;
using WorkTodo.Data.Exceptions;
using WorkTodo.Services;
using WorkTodo.Web.Components.Pages;
using WorkTodo.Web.Constants;

namespace WorkTodo.Web.Tests.Pages;

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
