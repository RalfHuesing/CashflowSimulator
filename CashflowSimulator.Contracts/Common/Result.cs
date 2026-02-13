namespace CashflowSimulator.Contracts.Common;

/// <summary>
/// Represents the result of an operation without a return value.
/// Use for expected failures (e.g. validation, I/O); throw for unexpected errors.
/// </summary>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string message) => new(false, message ?? string.Empty);
}
