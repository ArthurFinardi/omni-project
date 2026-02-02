namespace DeveloperStore.Application.Contracts;

public sealed record SaleItemDto(
    Guid Id,
    ExternalIdentityDto Product,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountRate,
    decimal DiscountAmount,
    decimal TotalAmount,
    bool IsCancelled);
