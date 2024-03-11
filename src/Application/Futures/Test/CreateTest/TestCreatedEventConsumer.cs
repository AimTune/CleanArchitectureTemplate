using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Futures.Test.CreateTest
{
    public class TestCreatedEventConsumer : IConsumer<TestCreatedEvent>
    {
        private readonly ILogger<TestCreatedEventConsumer> _logger;

        public TestCreatedEventConsumer(ILogger<TestCreatedEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<TestCreatedEvent> context)
        {
            _logger.LogInformation(context.Message.Name);

            return Task.CompletedTask;
        }
    }
}
