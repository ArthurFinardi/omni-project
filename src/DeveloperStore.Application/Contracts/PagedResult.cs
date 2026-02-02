namespace DeveloperStore.Application.Contracts;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Data,
    int TotalItems,
    int CurrentPage,
    int TotalPages);
