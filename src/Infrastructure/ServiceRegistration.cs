using Application.Abstractions.EventBus;
using Infrastructure.MessageBroker;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IEventBus, EventBus>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(
                    prefix: environment.IsDevelopment() ? $"dev-{Environment.MachineName}-" : "prod-",
                    includeNamespace: false));

            Type[] types = typeof(Application.AssemblyReference).Assembly.GetTypes();

            foreach (Type type in types)
            {
                AddEventConsumerAttribute? attribute = (AddEventConsumerAttribute)Attribute.GetCustomAttribute(type,
                       typeof(AddEventConsumerAttribute))!;

                if (attribute is null)
                    continue;

                if (attribute.ConsumerDefinitionType is { } defType)
                {
                    busConfigurator.AddConsumer(type, defType);
                }
                else
                {
                    busConfigurator.AddConsumer(type);
                }
            }

            busConfigurator.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        return services;
    }
}