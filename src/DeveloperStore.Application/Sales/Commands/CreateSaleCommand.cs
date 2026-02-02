using DeveloperStore.Application.Contracts;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands;

public sealed record CreateSaleCommand(
    string SaleNumber,
    DateTime SaleDate,
    ExternalIdentityDto Customer,
    ExternalIdentityDto Branch,
    IReadOnlyCollection<SaleItemInput> Items
) : IRequest<Result<SaleDto>>;
