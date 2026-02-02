using DeveloperStore.Application.Contracts;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries;

public sealed record GetSaleByIdQuery(Guid Id) : IRequest<Result<SaleDto>>;
