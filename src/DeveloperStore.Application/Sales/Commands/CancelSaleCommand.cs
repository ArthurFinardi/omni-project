using DeveloperStore.Application.Contracts;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands;

public sealed record CancelSaleCommand(Guid Id) : IRequest<Result<SaleDto>>;
