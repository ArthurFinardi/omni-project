namespace DeveloperStore.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
}
