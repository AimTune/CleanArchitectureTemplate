using Application.Behaviors;
using MediatR;
using FluentValidation;
using Infrastructure.MessageBroker;
using Microsoft.Extensions.Options;
using MassTransit;
using Application.Abstractions.EventBus;
using Application.Futures.Test.CreateTest;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistenceServices();

var presentationAssembly = typeof(Presentation.AssemblyReference).Assembly;

builder.Services.AddControllers()
    .AddApplicationPart(presentationAssembly);

var applicationAssembly = typeof(Application.AssemblyReference).Assembly;

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddValidatorsFromAssembly(applicationAssembly);

builder.Services
    .Configure<MessageBrokerSettings>(
    builder.Configuration.GetSection("MessageBroker"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    busConfigurator.AddConsumer<TestCreatedEventConsumer>();

    busConfigurator.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
