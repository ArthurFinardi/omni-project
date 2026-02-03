using DeveloperStore.Application.Abstractions;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Infra.Mongo.Models;
using MongoDB.Driver;

namespace DeveloperStore.Infra.Mongo.Repositories;

public sealed class SaleReadRepository : ISaleReadRepository
{
    private readonly IMongoCollection<SaleReadModel> _collection;

    public SaleReadRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SaleReadModel>("sales_read");
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await _collection.Find(s => s.Id == id).SingleOrDefaultAsync(cancellationToken);
        return model is null ? null : Map(model);
    }

    public async Task<PagedResult<SaleDto>> GetPagedAsync(
        int page,
        int size,
        string? order,
        IDictionary<string, string?> filters,
        CancellationToken cancellationToken)
    {
        var filter = BuildFilters(filters);
        var sort = BuildSort(order);

        var totalItems = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((page - 1) * size)
            .Limit(size)
            .ToListAsync(cancellationToken);

        var data = items.Select(Map).ToList();
        var totalPages = (int)Math.Ceiling(totalItems / (double)size);
        return new PagedResult<SaleDto>(data, totalItems, page, totalPages);
    }

    private static FilterDefinition<SaleReadModel> BuildFilters(IDictionary<string, string?> filters)
    {
        var builder = Builders<SaleReadModel>.Filter;
        var list = new List<FilterDefinition<SaleReadModel>>();

        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            switch (key)
            {
                case "saleNumber":
                    list.Add(ApplyStringFilter(builder, nameof(SaleReadModel.SaleNumber), value));
                    break;
                case "customer":
                    list.Add(ApplyStringFilter(builder, nameof(SaleReadModel.CustomerDescription), value));
                    break;
                case "branch":
                    list.Add(ApplyStringFilter(builder, nameof(SaleReadModel.BranchDescription), value));
                    break;
                case "isCancelled":
                    if (bool.TryParse(value, out var cancelled))
                    {
                        list.Add(builder.Eq(s => s.IsCancelled, cancelled));
                    }
                    break;
                case "_minSaleDate":
                    if (DateTime.TryParse(value, out var minDate))
                    {
                        list.Add(builder.Gte(s => s.SaleDate, minDate));
                    }
                    break;
                case "_maxSaleDate":
                    if (DateTime.TryParse(value, out var maxDate))
                    {
                        list.Add(builder.Lte(s => s.SaleDate, maxDate));
                    }
                    break;
                case "_minTotalAmount":
                    if (decimal.TryParse(value, out var minTotal))
                    {
                        list.Add(builder.Gte(s => s.TotalAmount, minTotal));
                    }
                    break;
                case "_maxTotalAmount":
                    if (decimal.TryParse(value, out var maxTotal))
                    {
                        list.Add(builder.Lte(s => s.TotalAmount, maxTotal));
                    }
                    break;
            }
        }

        return list.Count == 0 ? builder.Empty : builder.And(list);
    }

    private static FilterDefinition<SaleReadModel> ApplyStringFilter(
        FilterDefinitionBuilder<SaleReadModel> builder,
        string fieldName,
        string value)
    {
        FieldDefinition<SaleReadModel, string> field = fieldName;
        if (value.StartsWith('*') && value.EndsWith('*') && value.Length > 2)
        {
            var token = EscapeRegex(value.Trim('*'));
            return builder.Regex(field, new MongoDB.Bson.BsonRegularExpression(token, "i"));
        }

        if (value.StartsWith('*'))
        {
            var token = EscapeRegex(value.TrimStart('*'));
            return builder.Regex(field, new MongoDB.Bson.BsonRegularExpression($"{token}$", "i"));
        }

        if (value.EndsWith('*'))
        {
            var token = EscapeRegex(value.TrimEnd('*'));
            return builder.Regex(field, new MongoDB.Bson.BsonRegularExpression($"^{token}", "i"));
        }

        return builder.Eq(field, value);
    }

    private static string EscapeRegex(string token)
        => System.Text.RegularExpressions.Regex.Escape(token);

    private static SortDefinition<SaleReadModel> BuildSort(string? order)
    {
        var builder = Builders<SaleReadModel>.Sort;
        if (string.IsNullOrWhiteSpace(order))
        {
            return builder.Ascending(s => s.SaleNumber);
        }

        SortDefinition<SaleReadModel>? sort = null;
        foreach (var segment in order.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0];
            var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            sort = (field, desc, sort) switch
            {
                ("saleNumber", true, null) => builder.Descending(s => s.SaleNumber),
                ("saleNumber", false, null) => builder.Ascending(s => s.SaleNumber),
                ("saleDate", true, null) => builder.Descending(s => s.SaleDate),
                ("saleDate", false, null) => builder.Ascending(s => s.SaleDate),
                ("totalAmount", true, null) => builder.Descending(s => s.TotalAmount),
                ("totalAmount", false, null) => builder.Ascending(s => s.TotalAmount),
                ("saleNumber", true, not null) => sort!.Descending(s => s.SaleNumber),
                ("saleNumber", false, not null) => sort!.Ascending(s => s.SaleNumber),
                ("saleDate", true, not null) => sort!.Descending(s => s.SaleDate),
                ("saleDate", false, not null) => sort!.Ascending(s => s.SaleDate),
                ("totalAmount", true, not null) => sort!.Descending(s => s.TotalAmount),
                ("totalAmount", false, not null) => sort!.Ascending(s => s.TotalAmount),
                _ => sort ?? builder.Ascending(s => s.SaleNumber)
            };
        }

        return sort ?? builder.Ascending(s => s.SaleNumber);
    }

    private static SaleDto Map(SaleReadModel model)
        => new(
            model.Id,
            model.SaleNumber,
            model.SaleDate,
            new ExternalIdentityDto(model.CustomerExternalId, model.CustomerDescription),
            new ExternalIdentityDto(model.BranchExternalId, model.BranchDescription),
            model.TotalAmount,
            model.Items.Select(item => new SaleItemDto(
                item.Id,
                new ExternalIdentityDto(item.ProductExternalId, item.ProductDescription),
                item.Quantity,
                item.UnitPrice,
                item.DiscountRate,
                item.DiscountAmount,
                item.TotalAmount,
                item.IsCancelled)).ToList(),
            model.IsCancelled);
}
