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
        // Eckdaten (HelpKeyPrefix = "Eckdaten")
        ["Eckdaten"] = ("Eckdaten der Simulation", "Geburtsdatum, Renteneintritt und Lebenserwartung – Basis für die Simulation. Hier legen Sie die zeitlichen Eckdaten und das anfängliche liquide Kapital fest."),
        ["Eckdaten.DateOfBirth"] = ("Geburtsdatum", "Das Geburtsdatum wird für die Berechnung des aktuellen Alters sowie des Renteneintritts- und Enddatums der Simulation verwendet."),
        ["Eckdaten.RetirementAge"] = ("Renteneintritt (Alter)", "Alter in Jahren, in dem die Rente beginnt. Üblich sind 67 Jahre (Regelaltersrente)."),
        ["Eckdaten.LifeExpectancy"] = ("Lebenserwartung (Alter)", "Annahme für das Ende der Simulation in Lebensjahren. Die Simulation endet am Ersten des Monats, in dem dieses Alter erreicht wird."),
        ["Eckdaten.InitialLiquidCash"] = ("Start-Kapital (flüssig)", "Das Start-Kapital beschreibt das liquide Vermögen, das zum Beginn der Simulation (T0) zur Verfügung steht. Typischerweise sind dies Guthaben auf Tagesgeld- oder Girokonten, die noch nicht investiert sind. Hinweis: Schulden oder Kredite werden hier nicht negativ eingetragen, sondern in einem eigenen Bereich (Verbindlichkeiten) erfasst."),
        // Szenario (HelpKeyPrefix = "Szenario")
        ["Szenario"] = ("Szenario-Einstellungen", "Hier können Metadaten des aktuellen Szenarios bearbeitet werden."),
        ["Szenario.ScenarioName"] = ("Szenario Name", "Name des Szenarios zur besseren Unterscheidung mehrerer durchgerechneter Varianten."),
        // Einstellungen
        ["Einstellungen"] = ("Einstellungen", "Weitere Optionen für dieses Szenario."),
        // CashflowStreams (HelpKeyPrefix = "CashflowStreams")
        ["CashflowStreams"] = ("Laufende Cashflows", "Einnahmen oder Ausgaben mit wiederkehrendem Betrag und Intervall (monatlich/jährlich)."),
        ["CashflowStreams.Name"] = ("Bezeichnung", "Kurzer Name für diesen Cashflow (z. B. Gehalt, Miete)."),
        ["CashflowStreams.Amount"] = ("Betrag", "Wiederkehrender Betrag pro Intervall. Bei Einnahmen positiv, bei Ausgaben negativ eintragen."),
        ["CashflowStreams.Interval"] = ("Intervall", "Häufigkeit: Monatlich (Monthly) oder Jährlich (Yearly). Der Betrag wird jeweils pro Intervall angesetzt."),
        ["CashflowStreams.StartDate"] = ("Von (Datum)", "Startdatum des Cashflows. Ab diesem Datum wird der Betrag in der Simulation berücksichtigt."),
        ["CashflowStreams.EndDate"] = ("Bis (Datum)", "Enddatum des Cashflows. Leer lassen für unbegrenzte Laufzeit. Nach diesem Datum wird der Betrag nicht mehr angesetzt."),
        // CashflowEvents (HelpKeyPrefix = "CashflowEvents")
        ["CashflowEvents"] = ("Geplante Cashflow-Events", "Einmalige oder zeitlich begrenzte Einnahmen oder Ausgaben mit Zielmonat und optionalem Zeitfenster."),
        ["CashflowEvents.Name"] = ("Bezeichnung", "Kurzer Name für dieses Event (z. B. Urlaubsreise, Bonuszahlung)."),
        ["CashflowEvents.Amount"] = ("Betrag", "Einmaliger Betrag des Events. Bei Einnahmen positiv, bei Ausgaben negativ."),
        ["CashflowEvents.TargetDate"] = ("Zieldatum", "Planmonat, in dem das Event stattfinden soll. Dient als Referenz für den optionalen Toleranzbereich."),
        ["CashflowEvents.EarliestMonthOffset"] = ("Frühestens (Monate vom Zieldatum)", "Negative Zahl: Wie viele Monate vor dem Zieldatum das Event frühestens eintreten kann. 0 = nur im Zieldatum."),
        ["CashflowEvents.LatestMonthOffset"] = ("Spätestens (Monate vom Zieldatum)", "Positive Zahl: Wie viele Monate nach dem Zieldatum das Event spätestens eintreten kann. 0 = nur im Zieldatum."),
        // Marktdaten (HelpKeyPrefix = "Marktdaten")
        ["Marktdaten"] = ("Marktdaten", "Stochastische Marktfaktoren (z. B. Inflation, Aktienindex) steuern die Zufallspfade der Simulation. Hier legen Sie Faktoren mit Modelltyp (GBM oder Mean Reversion) und Parametern fest."),
        ["Marktdaten.Id"] = ("ID (eindeutig)", "Eindeutige Kennung des Faktors (z. B. Inflation_VPI). Wird in Korrelationen und zur Zuordnung in Cashflows verwendet. Keine Leerzeichen."),
        ["Marktdaten.Name"] = ("Name", "Anzeigename des Faktors (z. B. „Inflation (VPI)“ oder „Aktien Welt“). Erscheint in Listen und Auswahlfeldern."),
        ["Marktdaten.ModelType"] = ("Modelltyp", "GBM (Geometrische Brownsche Bewegung): typisch für Aktien/Indizes; keine Rückkehr zu einem Niveau. Ornstein-Uhlenbeck: typisch für Inflation/Zinsen; der Prozess kehrt zu einem langfristigen Mittelwert zurück."),
        ["Marktdaten.ExpectedReturn"] = ("Erwartete Rendite / Drift (μ)", "Langfristige erwartete Rendite bzw. Drift. Bei GBM: Drift der log-Renditen (z. B. 0,07 = 7 % p.a.). Bei Mean Reversion: Niveau, zu dem der Prozess reverts (z. B. 0,02 = 2 % Inflation)."),
        ["Marktdaten.Volatility"] = ("Volatilität (σ)", "Schwankungsbreite des Faktors. Je höher, desto riskanter. Typisch 0,15–0,20 für Aktien, geringer für Zinsen/Inflation. Parameter σ im Wiener-Prozess (annualisiert)."),
        ["Marktdaten.MeanReversionSpeed"] = ("Mean-Reversion-Speed (θ)", "Nur bei Ornstein-Uhlenbeck relevant. Steuert, wie schnell der Prozess zum langfristigen Mittelwert zurückkehrt. 0 = kaum Rückkehr (annähernd Random Walk); größere Werte = schnellere Anpassung."),
        ["Marktdaten.InitialValue"] = ("Initialwert", "Startwert des Faktors zum Simulationsstart (z. B. Indexstand 100 oder Inflationsrate 0,02). Einheit abhängig vom Faktor."),
        // Korrelationen (HelpKeyPrefix = "Korrelationen")
        ["Korrelationen"] = ("Korrelationen", "Paarweise Korrelationen zwischen Marktfaktoren. Die Werte fließen in die Korrelationsmatrix ein; diese muss positiv definit sein (wird beim Speichern geprüft)."),
        ["Korrelationen.FactorIdA"] = ("Faktor A", "Erster Faktor des Paars. Muss ein vorhandener Marktfaktor aus dem Bereich Marktdaten sein."),
        ["Korrelationen.FactorIdB"] = ("Faktor B", "Zweiter Faktor des Paars. Muss von Faktor A verschieden und ein vorhandener Marktfaktor sein."),
        ["Korrelationen.Correlation"] = ("Korrelation", "Pearson-Korrelation zwischen -1 und 1. 0 = unkorreliert; 1 = perfekt positiv; -1 = perfekt negativ. Die Gesamtmatrix aller Faktoren muss positiv definit bleiben."),
        // Dynamisierung in Cashflows
        ["CashflowStreams.EconomicFactorId"] = ("Dynamisierung / Marktfaktor", "Optional: Ein Marktfaktor (z. B. Inflation), an den dieser Cashflow gekoppelt wird. „Keine“ = nominaler Betrag ohne Dynamisierung."),
        ["CashflowEvents.EconomicFactorId"] = ("Dynamisierung / Marktfaktor", "Optional: Ein Marktfaktor (z. B. Inflation), an den dieses Event gekoppelt wird. „Keine“ = nominaler Betrag ohne Dynamisierung."),
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
