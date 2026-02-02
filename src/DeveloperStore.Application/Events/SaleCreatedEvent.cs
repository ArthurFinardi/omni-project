namespace DeveloperStore.Application.Events;

public sealed record SaleCreatedEvent(Guid SaleId, string SaleNumber);
