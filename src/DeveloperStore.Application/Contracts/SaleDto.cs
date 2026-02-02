namespace DeveloperStore.Application.Contracts;

public sealed record SaleDto(
    Guid Id,
    string SaleNumber,
    DateTime SaleDate,
    ExternalIdentityDto Customer,
    ExternalIdentityDto Branch,
    decimal TotalAmount,
    IReadOnlyCollection<SaleItemDto> Items,
    bool IsCancelled);
