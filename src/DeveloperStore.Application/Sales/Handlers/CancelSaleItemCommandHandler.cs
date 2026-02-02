using AutoMapper;
using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Commands;
using MediatR;

namespace DeveloperStore.Application.Sales.Handlers;

public sealed class CancelSaleItemCommandHandler : IRequestHandler<CancelSaleItemCommand, Result<SaleDto>>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;

    public CancelSaleItemCommandHandler(ISaleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<SaleDto>> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
        {
            return Result<SaleDto>.Fail("ResourceNotFound", "Sale not found", $"Sale with ID {request.SaleId} was not found.");
        }

        sale.CancelItem(request.ItemId);
        await _repository.UpdateAsync(sale, cancellationToken);

        return Result<SaleDto>.Ok(_mapper.Map<SaleDto>(sale));
    }
}
