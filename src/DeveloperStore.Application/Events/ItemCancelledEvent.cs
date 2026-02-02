namespace DeveloperStore.Application.Events;

public sealed record ItemCancelledEvent(Guid SaleId, Guid ItemId);
