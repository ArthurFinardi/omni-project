using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Xunit;

namespace DeveloperStore.Tests.Domain;

public sealed class SaleCancelTests
{
    [Fact]
    public void Cancel_marks_items_and_total_as_zero()
    {
        var sale = new Sale(
            "S-100",
            DateTime.UtcNow,
            new ExternalIdentity("cust-1", "Cliente 1"),
            new ExternalIdentity("branch-1", "Loja 1"));

        sale.AddItem(new SaleItem(
            new ExternalIdentity("prod-1", "Produto 1"),
            Quantity.From(4),
            new Money(10m)));

        sale.AddItem(new SaleItem(
            new ExternalIdentity("prod-2", "Produto 2"),
            Quantity.From(2),
            new Money(5m)));

        sale.Cancel();

        Assert.True(sale.IsCancelled);
        Assert.All(sale.Items, item => Assert.True(item.IsCancelled));
        Assert.Equal(0m, sale.TotalAmount.Amount);
    }
}
