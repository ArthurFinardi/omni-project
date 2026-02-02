namespace DeveloperStore.Application.Events;

public sealed record SaleModifiedEvent(Guid SaleId, string SaleNumber);
