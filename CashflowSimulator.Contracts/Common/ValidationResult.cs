namespace CashflowSimulator.Contracts.Common;

/// <summary>
/// Ergebnis einer Validierung: entweder gültig oder Liste von Fehlern.
/// Unabhängig von FluentValidation; die Validation-Schicht mappt darauf.
/// </summary>
public sealed class ValidationResult
{
    private static readonly ValidationResult SuccessInstance = new([]);

    public bool IsValid { get; }
    public IReadOnlyList<ValidationError> Errors { get; }

    private ValidationResult(IReadOnlyList<ValidationError> errors)
    {
        IsValid = errors.Count == 0;
        Errors = errors;
    }

    public static ValidationResult Success() => SuccessInstance;

    public static ValidationResult Failure(IEnumerable<ValidationError> errors)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0 ? SuccessInstance : new ValidationResult(list);
    }
}
