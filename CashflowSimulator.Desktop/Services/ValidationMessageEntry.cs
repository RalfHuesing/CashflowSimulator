namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Ein Eintrag für die zentrale Validierungsmeldungs-Anzeige (Source · Property: Message).
/// </summary>
public sealed class ValidationMessageEntry
{
    public string Source { get; }
    public string PropertyName { get; }
    public string Message { get; }

    public ValidationMessageEntry(string source, string propertyName, string message)
    {
        Source = source ?? string.Empty;
        PropertyName = propertyName ?? string.Empty;
        Message = message ?? string.Empty;
    }

    /// <summary>
    /// Anzeigetext z. B. "Eckdaten · Renteneintritt: Rente muss nach Geburt liegen."
    /// </summary>
    public string DisplayText => string.IsNullOrEmpty(PropertyName)
        ? $"{Source}: {Message}"
        : $"{Source} · {PropertyName}: {Message}";
}
