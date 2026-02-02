using AutoMapper;
using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Commands;
using MediatR;

namespace DeveloperStore.Application.Sales.Handlers;

public sealed class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, Result<SaleDto>>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleCommandHandler(ISaleRepository repository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<SaleDto>> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
        {
            return Result<SaleDto>.Fail("ResourceNotFound", "Sale not found", $"Sale with ID {request.Id} was not found.");
        }

        sale.Cancel();
        await _repository.UpdateAsync(sale, cancellationToken);
        await _eventPublisher.PublishAsync(new DeveloperStore.Application.Events.SaleCancelledEvent(sale.Id, sale.SaleNumber), cancellationToken);

        return Result<SaleDto>.Ok(_mapper.Map<SaleDto>(sale));
    }
}
