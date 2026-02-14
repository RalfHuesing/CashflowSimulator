namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Liefert zu einem HelpKey (z. B. Property-Name) Titel und Beschreibung für die Anzeige im InfoPanel.
/// </summary>
public interface IHelpProvider
{
    /// <summary>
    /// Versucht, Hilfetext für den angegebenen Schlüssel zu liefern.
    /// </summary>
    /// <param name="helpKey">Schlüssel (z. B. "InitialLiquidCash", "Eckdaten" für Seiten-Zero-State).</param>
    /// <param name="title">Kurzer Titel (z. B. Feldname).</param>
    /// <param name="description">Ausführliche Beschreibung.</param>
    /// <returns>True, wenn Eintrag gefunden.</returns>
    bool TryGetHelp(string helpKey, out string? title, out string? description);
}
