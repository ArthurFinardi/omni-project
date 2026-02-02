using DeveloperStore.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Api.Services;

public sealed class LogEventPublisher : IEventPublisher
{
    private readonly ILogger<LogEventPublisher> _logger;

    public LogEventPublisher(ILogger<LogEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Evento publicado: {EventType} {@Event}", typeof(TEvent).Name, @event);
        return Task.CompletedTask;
    }
}
