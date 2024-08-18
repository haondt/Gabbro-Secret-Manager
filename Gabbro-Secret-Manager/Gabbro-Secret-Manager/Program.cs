using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;


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
    .AddGabbroServices(builder.Configuration);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    IdentityModelEventSource.ShowPII = true;

app.UseStaticFiles();
app.UseCors(CORS_POLICY);
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

app.Run();
