using DeveloperStore.Application.Contracts;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries;

public sealed record GetSalesPagedQuery(
    int Page,
    int Size,
    string? Order,
    IDictionary<string, string?> Filters
) : IRequest<Result<PagedResult<SaleDto>>>;
