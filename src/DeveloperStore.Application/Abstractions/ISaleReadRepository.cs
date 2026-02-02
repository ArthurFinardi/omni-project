using DeveloperStore.Application.Contracts;

namespace DeveloperStore.Application.Abstractions;

public interface ISaleReadRepository
{
    Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<SaleDto>> GetPagedAsync(
        int page,
        int size,
        string? order,
        IDictionary<string, string?> filters,
        CancellationToken cancellationToken);
}
