using Microsoft.Extensions.DependencyInjection;
using WorkTodo.Data.Exceptions;
using WorkTodo.Data.Repositories;

namespace WorkTodo.Data.Tests;

public class DataLayerRegistrationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_or_blank_connection_string_is_rejected(string? connectionString)
    {
        var services = new ServiceCollection();

        Assert.Throws<MissingConnectionStringException>(() => services.AddDataLayer(connectionString));
    }

    [Fact]
    public void The_rejection_says_how_to_set_the_connection_string()
    {
        var exception = Assert.Throws<MissingConnectionStringException>(
            () => new ServiceCollection().AddDataLayer((string?)null));

        Assert.Contains("'WorkTodo'", exception.Message);
        Assert.Contains("ConnectionStrings__WorkTodo", exception.Message);
    }

    [Fact]
    public void A_connection_string_registers_every_repository()
    {
        var services = new ServiceCollection()
            .AddDataLayer("Server=.\\SQLEXPRESS;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.IsType<TodoRepository>(scope.ServiceProvider.GetRequiredService<ITodoRepository>());
        Assert.IsType<TagRepository>(scope.ServiceProvider.GetRequiredService<ITagRepository>());
        Assert.IsType<TodoDependencyRepository>(scope.ServiceProvider.GetRequiredService<ITodoDependencyRepository>());
    }
}
