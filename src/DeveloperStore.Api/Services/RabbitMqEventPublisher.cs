using DeveloperStore.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeveloperStore.Api.Services;

public sealed class RabbitMqEventPublisher : IEventPublisher
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly RabbitMqOptions _options;

    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger, IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug(
                "RabbitMQ publisher desabilitado. Evento ignorado: {EventType} {@Event}",
                typeof(TEvent).Name,
                @event);
            return Task.CompletedTask;
        }

        // Intencionalmente não publica em broker real (requisito do desafio).
        // Esta classe existe para demonstrar extensibilidade via configuração.
        _logger.LogInformation(
            "RabbitMQ (simulado): publicaria no exchange '{Exchange}' o evento {EventType} {@Event}",
            _options.Exchange,
            typeof(TEvent).Name,
            @event);

        return Task.CompletedTask;
    }
}

