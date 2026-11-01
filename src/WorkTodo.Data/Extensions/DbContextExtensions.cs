using Microsoft.EntityFrameworkCore;
using WorkTodo.Data.Constants;
using WorkTodo.Data.Exceptions;

namespace WorkTodo.Data.Extensions;

internal static class DbContextExtensions
{
    public static async Task SaveChangesOrResetAsync(this DbContext context, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            context.ChangeTracker.Clear();
            throw new DataAccessException(DatabaseConstants.SaveFailedMessage, exception);
        }
    }
}
