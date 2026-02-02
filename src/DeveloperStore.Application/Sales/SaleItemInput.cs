using DeveloperStore.Application.Contracts;

namespace DeveloperStore.Application.Sales;

public sealed record SaleItemInput(
    ExternalIdentityDto Product,
    int Quantity,
    decimal UnitPrice);
