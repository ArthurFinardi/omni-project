using AutoMapper;
using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Commands;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using MediatR;

namespace DeveloperStore.Application.Sales.Handlers;

public sealed class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Result<SaleDto>>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;

    public CreateSaleCommandHandler(ISaleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<SaleDto>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = new Sale(
            request.SaleNumber,
            request.SaleDate,
            new ExternalIdentity(request.Customer.ExternalId, request.Customer.Description),
            new ExternalIdentity(request.Branch.ExternalId, request.Branch.Description));

        foreach (var item in request.Items)
        {
            var saleItem = new SaleItem(
                new ExternalIdentity(item.Product.ExternalId, item.Product.Description),
                Quantity.From(item.Quantity),
                new Money(item.UnitPrice));

            sale.AddItem(saleItem);
        }

        await _repository.AddAsync(sale, cancellationToken);
        return Result<SaleDto>.Ok(_mapper.Map<SaleDto>(sale));
    }
}
