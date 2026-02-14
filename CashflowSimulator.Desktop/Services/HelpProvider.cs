using System.Collections.Frozen;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Statische Hilfetexte für Formularfelder und Seiten (Zero-State).
/// </summary>
public sealed class HelpProvider : IHelpProvider
{
    private static readonly FrozenDictionary<string, (string Title, string Description)> Entries = new Dictionary<string, (string, string)>(StringComparer.Ordinal)
    {
        ["BirthDate"] = ("Geburtsdatum", "Das Geburtsdatum wird für die Berechnung des aktuellen Alters sowie des Renteneintritts- und Enddatums der Simulation verwendet."),
        ["RetirementAge"] = ("Renteneintritt (Alter)", "Alter in Jahren, in dem die Rente beginnt. Üblich sind 67 Jahre (Regelaltersrente)."),
        ["LifeExpectancy"] = ("Lebenserwartung (Alter)", "Annahme für das Ende der Simulation in Lebensjahren. Die Simulation endet am Ersten des Monats, in dem dieses Alter erreicht wird."),
        ["InitialLiquidCash"] = ("Start-Kapital (flüssig)", "Das Start-Kapital beschreibt das liquide Vermögen, das zum Beginn der Simulation (T0) zur Verfügung steht. Typischerweise sind dies Guthaben auf Tagesgeld- oder Girokonten, die noch nicht investiert sind. Hinweis: Schulden oder Kredite werden hier nicht negativ eingetragen, sondern in einem eigenen Bereich (Verbindlichkeiten) erfasst."),
        ["Eckdaten"] = ("Eckdaten der Simulation", "Geburtsdatum, Renteneintritt und Lebenserwartung – Basis für die Simulation. Hier legen Sie die zeitlichen Eckdaten und das anfängliche liquide Kapital fest."),
        ["ScenarioName"] = ("Szenario Name", "Name des Szenarios zur besseren Unterscheidung mehrerer durchgerechneter Varianten."),
        ["Szenario"] = ("Szenario-Einstellungen", "Hier können Metadaten des aktuellen Szenarios bearbeitet werden."),
        ["SelectedTheme"] = ("Design (Theme)", "Wählen Sie das Farbschema und die Darstellung der Anwendung."),
        ["Einstellungen"] = ("Einstellungen", "Design und weitere Optionen für dieses Szenario."),
    }.ToFrozenDictionary();

    /// <inheritdoc />
    public bool TryGetHelp(string helpKey, out string? title, out string? description)
    {
        title = null;
        description = null;
        if (string.IsNullOrEmpty(helpKey))
            return false;

        if (!Entries.TryGetValue(helpKey, out var entry))
            return false;

        title = entry.Title;
        description = entry.Description;
        return true;
    }
}
