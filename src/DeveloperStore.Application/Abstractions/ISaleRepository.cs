using DeveloperStore.Domain.Entities;

namespace DeveloperStore.Application.Abstractions;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Sale> Items, int TotalItems)> GetPagedAsync(
        int page,
        int size,
        string? order,
        IDictionary<string, string?> filters,
        CancellationToken cancellationToken);
    Task AddAsync(Sale sale, CancellationToken cancellationToken);
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
