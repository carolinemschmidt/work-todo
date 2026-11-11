using WorkTodo.Data;
using WorkTodo.Data.Constants;
using WorkTodo.Data.Exceptions;
using WorkTodo.Services;
using WorkTodo.UI.Components.Layout;
using WorkTodo.Web.Components;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddDataLayer(builder.Configuration.GetConnectionString(DatabaseConstants.ConnectionStringName));
}
catch (MissingConnectionStringException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

builder.Services.AddServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Configuration.GetValue<bool>(DatabaseConstants.ApplyMigrationsSettingName))
{
    try
    {
        await app.Services.ApplyMigrationsAsync();
    }
    catch (DataAccessException exception)
    {
        app.Logger.LogCritical("{Message} Reason: {Reason}", exception.Message, exception.InnerException?.Message);
        return 1;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(MainLayout).Assembly);

app.Run();
return 0;
