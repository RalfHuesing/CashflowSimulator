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
        return GetStatusBarDisplayText(hasValidationMessages, validationMessageCount, null, currentFilePath);
    }

    /// <summary>
    /// Liefert den Statusleisten-Text: 0 Fehler = "Bereit" oder Dateipfad; 1 Fehler = Fehlertext; N &gt; 1 = "N Fehler gefunden".
    /// </summary>
    public static string GetStatusBarDisplayText(bool hasValidationMessages, int validationMessageCount, string? firstMessageDisplayText, string? currentFilePath)
    {
        if (!hasValidationMessages)
            return string.IsNullOrWhiteSpace(currentFilePath) ? "Bereit" : currentFilePath;
        if (validationMessageCount == 1)
            return string.IsNullOrWhiteSpace(firstMessageDisplayText) ? "1 Fehler gefunden" : firstMessageDisplayText;
        return $"{validationMessageCount} Fehler gefunden";
    }
}
