using System;
using DeveloperStore.Domain.Exceptions;
using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Domain.Entities;

public sealed class SaleItem
{
    private SaleItem()
    {
        Product = new ExternalIdentity(string.Empty, string.Empty);
        Quantity = Quantity.From(1);
        UnitPrice = Money.Zero;
        Discount = Discount.None;
        DiscountAmount = Money.Zero;
        TotalAmount = Money.Zero;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public ExternalIdentity Product { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Discount Discount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    public SaleItem(ExternalIdentity product, Quantity quantity, Money unitPrice)
    {
        Product = product;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    public void SetQuantity(Quantity quantity)
    {
        if (quantity.Value > 20)
        {
            throw new DomainException("Cannot sell more than 20 identical items.");
        }

        Quantity = quantity;
        ApplyDiscountRules();
    }

    public void SetUnitPrice(Money unitPrice)
    {
        UnitPrice = unitPrice;
        ApplyDiscountRules();
    }

    public void Cancel()
    {
        IsCancelled = true;
        Discount = Discount.None;
        DiscountAmount = Money.Zero;
        TotalAmount = Money.Zero;
    }

    private void ApplyDiscountRules()
    {
        if (Quantity.Value < 4)
        {
            Discount = Discount.None;
        }
        else if (Quantity.Value >= 10)
        {
            Discount = Discount.FromRate(0.20m);
        }
        else
        {
            Discount = Discount.FromRate(0.10m);
        }

        var gross = UnitPrice * Quantity.Value;
        DiscountAmount = gross * Discount.Rate;
        TotalAmount = new Money(gross.Amount - DiscountAmount.Amount);
    }
}
