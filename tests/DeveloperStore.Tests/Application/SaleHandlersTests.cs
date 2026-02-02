using AutoMapper;
using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Events;
using DeveloperStore.Application.Sales;
using DeveloperStore.Application.Sales.Commands;
using DeveloperStore.Application.Sales.Handlers;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace DeveloperStore.Tests.Application;

public sealed class SaleHandlersTests
{
    [Fact]
    public async Task CreateSale_publishes_event()
    {
        var repo = Substitute.For<ISaleRepository>();
        var mapper = BuildMapper();
        var publisher = Substitute.For<IEventPublisher>();

        var handler = new CreateSaleCommandHandler(repo, mapper, publisher);
        var command = new CreateSaleCommand(
            "S-123",
            DateTime.UtcNow,
            new ExternalIdentityDto("cust-1", "Cliente 1"),
            new ExternalIdentityDto("branch-1", "Loja 1"),
            new[]
            {
                new SaleItemInput(new ExternalIdentityDto("prod-1", "Produto 1"), 4, 10m)
            });

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        await publisher.Received(1).PublishAsync(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelSale_publishes_event()
    {
        var sale = new Sale(
            "S-999",
            DateTime.UtcNow,
            new ExternalIdentity("cust-1", "Cliente 1"),
            new ExternalIdentity("branch-1", "Loja 1"));

        var repo = Substitute.For<ISaleRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(sale);

        var mapper = BuildMapper();
        var publisher = Substitute.For<IEventPublisher>();

        var handler = new CancelSaleCommandHandler(repo, mapper, publisher);
        var result = await handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        Assert.True(result.Success);
        await publisher.Received(1).PublishAsync(Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    private static IMapper BuildMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new SaleMappingProfile()));
        return config.CreateMapper();
    }
}
