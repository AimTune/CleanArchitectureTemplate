using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Futures.Test.CreateTest;

[AddEventConsumer(typeof(TestCreatedEventConsumerDefinition))]
public class TestCreatedEventConsumer(ILogger<TestCreatedEventConsumer> logger) : IConsumer<TestCreatedEvent>
{

    public Task Consume(ConsumeContext<TestCreatedEvent> context)
    {
        logger.LogWarning(message: context.Message.Name);

        return Task.CompletedTask;
    }
}

public class TestCreatedEventConsumerDefinition
    : ConsumerDefinition<TestCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<TestCreatedEventConsumer> consumerConfigurator,
            IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(x => x.Interval(30, 3000));
        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator, context);
    }
}