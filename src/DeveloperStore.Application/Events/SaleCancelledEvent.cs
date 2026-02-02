namespace DeveloperStore.Application.Events;

public sealed record SaleCancelledEvent(Guid SaleId, string SaleNumber);
