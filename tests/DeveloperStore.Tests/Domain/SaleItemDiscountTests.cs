using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Exceptions;
using DeveloperStore.Domain.ValueObjects;
using Xunit;

namespace DeveloperStore.Tests.Domain;

public sealed class SaleItemDiscountTests
{
    [Theory]
    [InlineData(1, 0.00)]
    [InlineData(3, 0.00)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void Applies_discount_rules_correctly(int quantity, decimal expectedRate)
    {
        var item = new SaleItem(
            new ExternalIdentity("prod-1", "Produto 1"),
            Quantity.From(quantity),
            new Money(10m));

        Assert.Equal(expectedRate, item.Discount.Rate);
    }

    [Fact]
    public void Rejects_quantity_above_twenty()
    {
        Assert.Throws<DomainException>(() =>
            new SaleItem(
                new ExternalIdentity("prod-1", "Produto 1"),
                Quantity.From(21),
                new Money(10m)));
    }
}
