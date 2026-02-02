using DeveloperStore.Application.Abstractions;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Infra.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly SalesDbContext _context;

    public SaleRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Sale> Items, int TotalItems)> GetPagedAsync(
        int page,
        int size,
        string? order,
        IDictionary<string, string?> filters,
        CancellationToken cancellationToken)
    {
        var query = _context.Sales.Include(s => s.Items).AsQueryable();

        query = ApplyFilters(query, filters);
        query = ApplyOrdering(query, order);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken)
    {
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sale is null)
        {
            return false;
        }

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, IDictionary<string, string?> filters)
    {
        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            switch (key)
            {
                case "saleNumber":
                    query = ApplyStringFilter(query, value, s => s.SaleNumber);
                    break;
                case "customer":
                    query = ApplyStringFilter(query, value, s => s.Customer.Description);
                    break;
                case "branch":
                    query = ApplyStringFilter(query, value, s => s.Branch.Description);
                    break;
                case "isCancelled":
                    if (bool.TryParse(value, out var cancelled))
                    {
                        query = query.Where(s => s.IsCancelled == cancelled);
                    }
                    break;
                case "_minSaleDate":
                    if (DateTime.TryParse(value, out var minDate))
                    {
                        query = query.Where(s => s.SaleDate >= minDate);
                    }
                    break;
                case "_maxSaleDate":
                    if (DateTime.TryParse(value, out var maxDate))
                    {
                        query = query.Where(s => s.SaleDate <= maxDate);
                    }
                    break;
                case "_minTotalAmount":
                    if (decimal.TryParse(value, out var minTotal))
                    {
                        query = query.Where(s => s.TotalAmount.Amount >= minTotal);
                    }
                    break;
                case "_maxTotalAmount":
                    if (decimal.TryParse(value, out var maxTotal))
                    {
                        query = query.Where(s => s.TotalAmount.Amount <= maxTotal);
                    }
                    break;
            }
        }

        return query;
    }

    private static IQueryable<Sale> ApplyStringFilter(IQueryable<Sale> query, string value, Func<Sale, string> selector)
    {
        if (value.StartsWith('*') && value.EndsWith('*') && value.Length > 2)
        {
            var token = value.Trim('*');
            return query.Where(s => EF.Functions.ILike(selector(s), $"%{token}%"));
        }

        if (value.StartsWith('*'))
        {
            var token = value.TrimStart('*');
            return query.Where(s => EF.Functions.ILike(selector(s), $"%{token}"));
        }

        if (value.EndsWith('*'))
        {
            var token = value.TrimEnd('*');
            return query.Where(s => EF.Functions.ILike(selector(s), $"{token}%"));
        }

        return query.Where(s => EF.Functions.ILike(selector(s), value));
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
        {
            return query.OrderBy(s => s.SaleNumber);
        }

        IOrderedQueryable<Sale>? ordered = null;
        foreach (var segment in order.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0];
            var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = (field, desc, ordered) switch
            {
                ("saleNumber", true, null) => query.OrderByDescending(s => s.SaleNumber),
                ("saleNumber", false, null) => query.OrderBy(s => s.SaleNumber),
                ("saleDate", true, null) => query.OrderByDescending(s => s.SaleDate),
                ("saleDate", false, null) => query.OrderBy(s => s.SaleDate),
                ("totalAmount", true, null) => query.OrderByDescending(s => s.TotalAmount.Amount),
                ("totalAmount", false, null) => query.OrderBy(s => s.TotalAmount.Amount),
                ("saleNumber", true, not null) => ordered!.ThenByDescending(s => s.SaleNumber),
                ("saleNumber", false, not null) => ordered!.ThenBy(s => s.SaleNumber),
                ("saleDate", true, not null) => ordered!.ThenByDescending(s => s.SaleDate),
                ("saleDate", false, not null) => ordered!.ThenBy(s => s.SaleDate),
                ("totalAmount", true, not null) => ordered!.ThenByDescending(s => s.TotalAmount.Amount),
                ("totalAmount", false, not null) => ordered!.ThenBy(s => s.TotalAmount.Amount),
                _ => ordered ?? query.OrderBy(s => s.SaleNumber)
            };
        }

        return ordered ?? query.OrderBy(s => s.SaleNumber);
    }
}
