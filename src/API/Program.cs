using API;
using Application;
using Infrastructure;
using Persistence;
using Scalar.AspNetCore;
using Shared.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration, builder.Environment)
    .AddPersistenceServices()
    .AddPresentationServices(builder.Configuration)
    .AddOpenTelemetryExtensions(builder.Configuration);



WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
            .WithTitle("Example API")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDownloadButton(true)
            .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios)
            .WithCustomCss("")
            .WithSidebar(true)
            .WithHttpBearerAuthentication(new HttpBearerOptions {
                Token = ""
            })
    );
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/test", () => "Hello World!").RequireAuthorization();

app.Run();