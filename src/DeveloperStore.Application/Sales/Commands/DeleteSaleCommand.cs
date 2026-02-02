using DeveloperStore.Application.Contracts;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands;

public sealed record DeleteSaleCommand(Guid Id) : IRequest<Result<bool>>;
