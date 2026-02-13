namespace CashflowSimulator.Contracts.Common;

/// <summary>
/// Represents the result of an operation with a return value.
/// Use for expected failures (e.g. load/save); throw for unexpected errors.
/// </summary>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string message) => new(false, default, message ?? string.Empty);
}
