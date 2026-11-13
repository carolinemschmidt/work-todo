using WorkTodo.Data.Constants;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.Extensions;

namespace WorkTodo.Data.Tests;

public class DatabaseErrorTests
{
    [Theory]
    [InlineData(2601)]
    [InlineData(2627)]
    public void Duplicate_key_and_duplicate_index_errors_are_recognised(int sqlErrorNumber)
    {
        Assert.True(SqlServerErrors.IsDuplicateKey(sqlErrorNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(547)]
    [InlineData(-2)]
    [InlineData(53)]
    public void Other_sql_errors_are_not_treated_as_duplicates(int sqlErrorNumber)
    {
        Assert.False(SqlServerErrors.IsDuplicateKey(sqlErrorNumber));
    }

    [Fact]
    public void A_duplicate_entry_is_a_data_access_exception_with_a_message_that_says_what_happened()
    {
        var exception = new DuplicateEntryException(new InvalidOperationException("provider detail"));

        Assert.IsAssignableFrom<DataAccessException>(exception);
        Assert.Equal(DatabaseConstants.DuplicateEntryMessage, exception.Message);
        Assert.DoesNotContain("provider detail", exception.Message);
    }

    [Theory]
    [InlineData(nameof(DatabaseConstants.MigrationFailedMessage))]
    [InlineData(nameof(DatabaseConstants.MissingConnectionStringMessage))]
    public void Messages_name_the_connection_string_the_app_reads(string constantName)
    {
        var message = (string)typeof(DatabaseConstants).GetField(constantName)!.GetRawConstantValue()!;

        Assert.Contains($"'{DatabaseConstants.ConnectionStringName}'", message);
    }
}
