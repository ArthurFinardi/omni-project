using DeveloperStore.Application.Contracts;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands;

public sealed record CancelSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest<Result<SaleDto>>;
