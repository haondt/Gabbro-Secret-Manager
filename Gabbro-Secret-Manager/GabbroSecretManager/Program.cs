using GabbroSecretManager.Domain.Shared.Extensions;
using GabbroSecretManager.Persistence.Extensions;
using GabbroSecretManager.UI.Shared.Extensions;
using GabbroSecretManager.UI.Shared.Middlewares;
using Haondt.Web.Core.Middleware;
using Haondt.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration)
    .AddGabbroSecretManagerPersistenceServices(builder.Configuration)
    .AddGabbroSecretManagerServices(builder.Configuration)
    .AddGabbroSecretManagerUI(builder.Configuration);

builder.Services.AddMvc();
builder.Services.AddServerSideBlazor();



var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<UnmappedRouteHandlerMiddleware>();

app.Services.PerformDatabaseMigrations();


app.Run();
