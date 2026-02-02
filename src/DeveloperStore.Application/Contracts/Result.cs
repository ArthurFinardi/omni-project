namespace DeveloperStore.Application.Contracts;

public sealed record Result<T>(bool Success, T? Data, string? ErrorType, string? Error, string? Detail)
{
    public static Result<T> Ok(T data) => new(true, data, null, null, null);

    public static Result<T> Fail(string errorType, string error, string detail)
        => new(false, default, errorType, error, detail);
}
