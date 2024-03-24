using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
const string CORS_POLICY = "_gabbroSecretManagerPolicy";

builder.Services.AddControllers();
builder.Services.AddMvc();
builder.Services.AddCors(o => o.AddPolicy(CORS_POLICY, p =>
{
    p.AllowAnyOrigin();
    p.AllowAnyHeader();
}));

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddCoreServices(builder.Configuration)
    .AddGabbroServices(builder.Configuration)
    .RegisterPages()
    .RegisterPartialPages();

var app = builder.Build();
app.UseStaticFiles();
app.UseCors(CORS_POLICY);
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

app.Run();
