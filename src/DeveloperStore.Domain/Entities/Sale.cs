using System;
using System.Collections.Generic;
using System.Linq;
using DeveloperStore.Domain.ValueObjects;

namespace DeveloperStore.Domain.Entities;

public sealed class Sale
{
    private readonly List<SaleItem> _items = new();

    private Sale()
    {
        SaleNumber = string.Empty;
        Customer = new ExternalIdentity(string.Empty, string.Empty);
        Branch = new ExternalIdentity(string.Empty, string.Empty);
        TotalAmount = Money.Zero;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string SaleNumber { get; private set; }
    public DateTime SaleDate { get; private set; }
    public ExternalIdentity Customer { get; private set; }
    public ExternalIdentity Branch { get; private set; }
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();
    public Money TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    public Sale(string saleNumber, DateTime saleDate, ExternalIdentity customer, ExternalIdentity branch)
    {
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        Customer = customer;
        Branch = branch;
        RecalculateTotal();
    }

    public void AddItem(SaleItem item)
    {
        _items.Add(item);
        RecalculateTotal();
    }

    public void UpdateDetails(string saleNumber, DateTime saleDate, ExternalIdentity customer, ExternalIdentity branch)
    {
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        Customer = customer;
        Branch = branch;
    }

    public void ReplaceItems(IEnumerable<SaleItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        RecalculateTotal();
    }

    public void Cancel()
    {
        IsCancelled = true;
        foreach (var item in _items)
        {
            item.Cancel();
        }
        RecalculateTotal();
    }

    public void CancelItem(Guid itemId)
    {
        var item = _items.Single(i => i.Id == itemId);
        item.Cancel();
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled)
            .Select(i => i.TotalAmount)
            .Aggregate(Money.Zero, (current, next) => current + next);
    }
}
