using AutoMapper;
using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Queries;
using MediatR;

namespace DeveloperStore.Application.Sales.Handlers;

public sealed class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, Result<SaleDto>>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;

    public GetSaleByIdQueryHandler(ISaleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<SaleDto>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
        {
            return Result<SaleDto>.Fail("ResourceNotFound", "Sale not found", $"Sale with ID {request.Id} was not found.");
        }

        return Result<SaleDto>.Ok(_mapper.Map<SaleDto>(sale));
    }
}
