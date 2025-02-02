using API;
using API.Controllers;
using Application.Abstractions.EventBus;
using Application.Behaviors;
using Application.Futures.Test.CreateTest;
using FluentValidation;
using Infrastructure.MessageBroker;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistenceServices();

Assembly presentationAssembly = typeof(Presentation.AssemblyReference).Assembly;

builder.Services.AddControllers()
    .AddApplicationPart(presentationAssembly);

Assembly applicationAssembly = typeof(Application.AssemblyReference).Assembly;

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddValidatorsFromAssembly(applicationAssembly);

builder.Services
    .Configure<MessageBrokerSettings>(
    builder.Configuration.GetSection("MessageBroker"));

IConfigurationSection jwtConfig = builder.Configuration.GetSection("Jwt");
byte[] key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddScoped<TokenService>();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    busConfigurator.AddConsumer<TestCreatedEventConsumer>();

    busConfigurator.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));

    /*
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        MessageBrokerSettings settings = context.GetRequiredService<MessageBrokerSettings>();

        configurator.Host(new Uri(settings.Host), h =>
        {
            h.Username(settings.Username);
            h.Password(settings.Password);
        });
    });
    */
});

builder.Services.AddTransient<IEventBus, EventBus>();

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
