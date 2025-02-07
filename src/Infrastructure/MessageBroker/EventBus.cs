using Application.Abstractions.EventBus;
using MassTransit;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Infrastructure.MessageBroker;

public sealed class EventBus(IPublishEndpoint publishEndpoint, IHttpContextAccessor httpContextAccessor) : IEventBus
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {

        return publishEndpoint.Publish(message, context =>
        {
            string? correlationIdString = Activity.Current?.TraceId.ToString();

            HttpContext? httpContext = httpContextAccessor.HttpContext;

            if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out Microsoft.Extensions.Primitives.StringValues headerCorrelationId))
            {
                correlationIdString = headerCorrelationId.ToString();
            }

            if (correlationIdString != null && Guid.TryParse(correlationIdString, out Guid correlationId))
            {
                context.Headers.Set("X-Correlation-ID", correlationIdString);
                context.CorrelationId = correlationId;
            }
        }, cancellationToken);
    }
}