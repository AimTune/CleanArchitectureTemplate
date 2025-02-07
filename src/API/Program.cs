using API;
using API.Controllers;
using API.Extensions;
using Application;
using Infrastructure;
using Infrastructure.MessageBroker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Scalar.AspNetCore;
using Shared.Extensions;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration, builder.Environment)
    .AddPersistenceServices()
    .AddOptionsExtensions(builder.Configuration)
    .AddOpenTelemetryExtensions(builder.Configuration);

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly);

IConfigurationSection jwtConfig = builder.Configuration.GetSection("Jwt");
byte[] key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddScoped<TokenService>();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);


// services.AddTransient<ExceptionHandlingMiddleware>();
builder
    .Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi(opt => opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorizationBuilder();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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