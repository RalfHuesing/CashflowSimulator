namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Zentrale Logik für den Statusleisten-Text (testbar ohne ViewModel-Abhängigkeiten).
/// </summary>
public static class StatusBarTextProvider
{
    /// <summary>
    /// Liefert den anzuzeigenden Text: bei Fehlern "{count} Fehler gefunden", sonst Dateipfad oder "Bereit".
    /// </summary>
    public static string GetStatusText(bool hasValidationMessages, int validationMessageCount, string? currentFilePath)
    {
        if (hasValidationMessages)
            return $"{validationMessageCount} Fehler gefunden";
        return string.IsNullOrWhiteSpace(currentFilePath) ? "Bereit" : currentFilePath;
    }
}
