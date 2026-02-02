using AutoMapper;
using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Commands;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using MediatR;

namespace DeveloperStore.Application.Sales.Handlers;

public sealed class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand, Result<SaleDto>>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public UpdateSaleCommandHandler(ISaleRepository repository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<SaleDto>> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
        {
            return Result<SaleDto>.Fail("ResourceNotFound", "Sale not found", $"Sale with ID {request.Id} was not found.");
        }

        sale.UpdateDetails(
            request.SaleNumber,
            request.SaleDate,
            new ExternalIdentity(request.Customer.ExternalId, request.Customer.Description),
            new ExternalIdentity(request.Branch.ExternalId, request.Branch.Description));

        var items = request.Items.Select(item =>
            new SaleItem(
                new ExternalIdentity(item.Product.ExternalId, item.Product.Description),
                Quantity.From(item.Quantity),
                new Money(item.UnitPrice)));

        sale.ReplaceItems(items);
        await _repository.UpdateAsync(sale, cancellationToken);
        await _eventPublisher.PublishAsync(new DeveloperStore.Application.Events.SaleModifiedEvent(sale.Id, sale.SaleNumber), cancellationToken);

        return Result<SaleDto>.Ok(_mapper.Map<SaleDto>(sale));
    }
}
