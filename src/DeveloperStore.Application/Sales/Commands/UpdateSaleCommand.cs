using DeveloperStore.Application.Contracts;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands;

public sealed record UpdateSaleCommand(
    Guid Id,
    string SaleNumber,
    DateTime SaleDate,
    ExternalIdentityDto Customer,
    ExternalIdentityDto Branch,
    IReadOnlyCollection<SaleItemInput> Items
) : IRequest<Result<SaleDto>>;
