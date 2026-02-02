using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Commands;
using MediatR;

namespace DeveloperStore.Application.Sales.Handlers;

public sealed class DeleteSaleCommandHandler : IRequestHandler<DeleteSaleCommand, Result<bool>>
{
    private readonly ISaleRepository _repository;

    public DeleteSaleCommandHandler(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
        {
            return Result<bool>.Fail("ResourceNotFound", "Sale not found", $"Sale with ID {request.Id} was not found.");
        }

        return Result<bool>.Ok(true);
    }
}
