using AutoMapper;
using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Queries;
using MediatR;

namespace DeveloperStore.Application.Sales.Handlers;

public sealed class GetSalesPagedQueryHandler : IRequestHandler<GetSalesPagedQuery, Result<PagedResult<SaleDto>>>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;

    public GetSalesPagedQueryHandler(ISaleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<SaleDto>>> Handle(GetSalesPagedQuery request, CancellationToken cancellationToken)
    {
        var (items, totalItems) = await _repository.GetPagedAsync(
            request.Page,
            request.Size,
            request.Order,
            request.Filters,
            cancellationToken);

        var data = items.Select(s => _mapper.Map<SaleDto>(s)).ToList();
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.Size);

        return Result<PagedResult<SaleDto>>.Ok(
            new PagedResult<SaleDto>(data, totalItems, request.Page, totalPages));
    }
}
