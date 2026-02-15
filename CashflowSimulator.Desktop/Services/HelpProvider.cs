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
        ["Eckdaten"] = ("Eckdaten der Simulation", "Geburtsdatum, Lebenserwartung, Verlustvorträge und Start-Kapital – Basis für die Simulation."),
        ["Eckdaten.DateOfBirth"] = ("Geburtsdatum", "Das Geburtsdatum wird für die Berechnung des aktuellen Alters und des Enddatums der Simulation verwendet."),
        ["Eckdaten.LifeExpectancy"] = ("Lebenserwartung (Alter)", "Annahme für das Ende der Simulation in Lebensjahren. Die Simulation endet am Ersten des Monats, in dem dieses Alter erreicht wird."),
        ["Eckdaten.InitialLossCarryforwardGeneral"] = ("Verlustvortrag allgemein", "Bestehender steuerlicher Verlustvortrag (allgemeiner Verlusttopf) zum Simulationsstart. Wird mit sonstigen Gewinnen verrechnet; Gewinne im ersten Jahr mindern zuerst diesen Topf."),
        ["Eckdaten.InitialLossCarryforwardStocks"] = ("Verlustvortrag Aktien", "Bestehender steuerlicher Verlustvortrag (Aktienverlusttopf) zum Simulationsstart. Nur mit Aktiengewinnen verrechenbar."),
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
        ["CashflowStreams.Interval"] = ("Intervall", "Häufigkeit: Monatlich oder Jährlich. Der Betrag wird jeweils pro Intervall angesetzt."),
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
        // Anlageklassen (HelpKeyPrefix = "AssetClasses")
        ["AssetClasses"] = ("Anlageklassen", "Hier legen Sie Ihre strategische Zielallokation fest: Welche „Buckets“ (z. B. Aktien Welt, Anleihen) sollen wie stark gewichtet sein? Die Summe sollte 100 % ergeben."),
        ["AssetClasses.Name"] = ("Name", "Was ist eine Anlageklasse? Eine Anlageklasse ist ein Oberbegriff für eine Gruppe von Anlagen, z. B. „Aktien Welt“ oder „Sicherheitsbaustein“. So behalten Sie Ihre Strategie im Blick."),
        ["AssetClasses.TargetWeight"] = ("Zielgewichtung", "Anteil dieser Anlageklasse am Gesamtportfolio (0–100 %). Beispiel: 70 % Aktien Welt, 30 % Anleihen. Die Summe aller Klassen sollte 100 % ergeben."),
        ["AssetClasses.Color"] = ("Farbe", "Optionale Hex-Farbe (z. B. #1E88E5) für Diagramme und Übersichten. Kann leer bleiben."),
        // Vermögenswerte / Portfolio (HelpKeyPrefix = "Portfolio")
        ["Portfolio"] = ("Vermögenswerte", "Konkrete Produkte (ETFs, Konten): Hier pflegen Sie Stammdaten, aktuellen Kurs und die Zuordnung zu Anlageklasse und Marktfaktor. Transaktionen erfassen Sie im Bereich Transaktionen."),
        ["Portfolio.Name"] = ("Name", "Bezeichnung des Vermögenswerts, z. B. „Vanguard FTSE All-World“ oder „Tagesgeld Notgroschen“."),
        ["Portfolio.Isin"] = ("ISIN", "Internationale Wertpapierkennnummer (12 Zeichen). Optional, hilft aber bei der eindeutigen Zuordnung und ggf. Kursabfrage."),
        ["Portfolio.AssetType"] = ("Typ", "Art des Vermögenswerts: ETF, Anleihe, Bargeld, Krypto oder Immobilie. Beeinflusst Anzeige und ggf. Steuerlogik."),
        ["Portfolio.EconomicFactorId"] = ("Marktfaktor", "Welcher Marktfaktor steuert die Wertentwicklung? Alle Assets mit demselben Faktor (z. B. Aktien Welt) folgen demselben simulierten Kursverlauf."),
        ["Portfolio.AssetClassId"] = ("Anlageklasse", "Zuordnung zu einer Ihrer Anlageklassen (Strategy). So sehen Sie, welches Produkt zu welchem „Bucket“ gehört (z. B. welcher ETF zu „Aktien Welt“)."),
        ["Portfolio.CurrentPrice"] = ("Aktueller Kurs", "Warum brauche ich den aktuellen Kurs? Der Stückpreis ist der Startwert für die Simulation und die Basis für die Anzeige des Gesamtwerts (Kurs × Stückzahl). Pflegen Sie hier den letzten bekannten Kurs."),
        ["Portfolio.TaxType"] = ("Steuerart", "Was bewirkt die Teilfreistellung? Bei Aktienfonds gilt in Deutschland eine 30%ige Teilfreistellung auf Veräußerungsgewinn und Ausschüttung. Anleihenfonds haben keine Teilfreistellung. Wählen Sie die passende Kategorie."),
        ["Portfolio.IsActiveSavingsInstrument"] = ("Aktiv bespart", "Nur ein Asset pro Sparlogik sollte „aktiv bespart“ sein: Neue Sparraten fließen in dieses Produkt. Altbestände, die nicht mehr bespart werden, haben „nein“."),
        // Transaktionen (HelpKeyPrefix = "Transactions")
        ["Transactions"] = ("Transaktionen", "Zentrales Journal aller Käufe und Verkäufe über alle Vermögenswerte. Hier erfassen Sie jede Buchung am richtigen Asset. Sortierung: neueste zuerst."),
        ["Transactions.AssetId"] = ("Vermögenswert", "Zu welchem Produkt (ETF, Konto) gehört diese Transaktion? Wählen Sie das passende Asset aus der Liste."),
        ["Transactions.Date"] = ("Datum", "Wertstellungs- oder Handelsdatum der Transaktion. Wichtig für die chronologische Historie und die FIFO-Steuerberechnung bei Verkäufen."),
        ["Transactions.TransactionType"] = ("Typ", "Kauf, Verkauf, Ausschüttung oder Vorabpauschale. Bestimmt die Buchungslogik (z. B. bei Kauf erhöht sich die Stückzahl)."),
        ["Transactions.Quantity"] = ("Menge", "Anzahl der Anteile/Stück bei Kauf oder Verkauf. Bei Ausschüttung/Vorabpauschale kann 0 oder eine rechnerische Menge stehen."),
        ["Transactions.PricePerUnit"] = ("Preis pro Stück", "Kurs zum Transaktionszeitpunkt (z. B. Einstandskurs bei Kauf). Wird zusammen mit der Menge für den Gesamtbetrag herangezogen."),
        ["Transactions.TotalAmount"] = ("Gesamtbetrag", "Gesamtwert der Transaktion (z. B. Menge × Preis bei Kauf/Verkauf) oder Ausschüttungsbetrag in Währung."),
        ["Transactions.TaxAmount"] = ("Steueranteil", "Falls bei dieser Transaktion bereits Steuer angefallen ist (z. B. Kapitalertragsteuer auf Ausschüttung), hier eintragen. Sonst 0."),
        // Dynamisierung in Cashflows
        ["CashflowStreams.EconomicFactorId"] = ("Dynamisierung / Marktfaktor", "Optional: Ein Marktfaktor (z. B. Inflation), an den dieser Cashflow gekoppelt wird. „Keine“ = nominaler Betrag ohne Dynamisierung."),
        ["CashflowEvents.EconomicFactorId"] = ("Dynamisierung / Marktfaktor", "Optional: Ein Marktfaktor (z. B. Inflation), an den dieses Event gekoppelt wird. „Keine“ = nominaler Betrag ohne Dynamisierung."),
        // Steuerprofile (HelpKeyPrefix = "TaxProfiles")
        ["TaxProfiles"] = ("Steuerprofile", "Steuersätze und Freibeträge pro Lebensphase (z. B. Erwerbsleben vs. Rente). Jede Lebensphase verweist auf ein Steuer-Profil."),
        ["TaxProfiles.Id"] = ("ID (eindeutig)", "Eindeutige Kennung des Steuer-Profils. Wird von Lebensphasen referenziert. Keine Leerzeichen."),
        ["TaxProfiles.Name"] = ("Name", "Anzeigename des Profils (z. B. „Standard (Erwerb)“ oder „Rentenbesteuerung“)."),
        ["TaxProfiles.CapitalGainsTaxRate"] = ("Kapitalertragsteuer-Satz", "Satz für Kapitalerträge (inkl. Soli, Kirchensteuer) als Dezimal 0–1. Beispiel: 0,26375 = 26,375 % (Abgeltungsteuer in Deutschland)."),
        ["TaxProfiles.TaxFreeAllowance"] = ("Freibetrag", "Sparerpauschbetrag bzw. Freibetrag für Kapitalerträge in Euro (z. B. 1000 €)."),
        ["TaxProfiles.IncomeTaxRate"] = ("Einkommensteuer-Satz", "Satz für nachgelagerte Besteuerung (z. B. Rente) als Dezimal 0–1. In der Rente oft niedriger als im Erwerbsleben."),
        // Strategieprofile (HelpKeyPrefix = "StrategyProfiles")
        ["StrategyProfiles"] = ("Strategieprofile", "Liquiditätsreserve, Rebalancing und Lookahead pro Lebensphase (z. B. Aufbau vs. Entnahme). Werden von Lebensphasen referenziert."),
        ["StrategyProfiles.Id"] = ("ID (eindeutig)", "Eindeutige Kennung des Strategie-Profils. Wird von Lebensphasen referenziert."),
        ["StrategyProfiles.Name"] = ("Name", "Anzeigename des Profils (z. B. „Aufbau“ oder „Entnahme“)."),
        ["StrategyProfiles.CashReserveMonths"] = ("Liquiditätsreserve (Monate)", "Anzahl Monatsausgaben als liquide Reserve (Notgroschen). Typisch 3–12 Monate."),
        ["StrategyProfiles.RebalancingThreshold"] = ("Rebalancing-Schwelle", "Abweichungsschwelle (z. B. 0,05 = 5 %), ab der umgeschichtet wird. Größere Werte = selteneres Rebalancing."),
        ["StrategyProfiles.MinimumTransactionAmount"] = ("Mindest-Transaktionsgröße", "Orders unter diesem Betrag werden beim Rebalancing nicht ausgeführt (Schutz vor Gebühren durch Mikro-Transaktionen)."),
        ["StrategyProfiles.LookaheadMonths"] = ("Lookahead (Monate)", "Wie viele Monate voraus auf geplante Events (Cashflow-Events) gespart wird. Relevant für Liquiditätsplanung."),
        // Lebensphasen (HelpKeyPrefix = "LifecyclePhases")
        ["LifecyclePhases"] = ("Lebensphasen", "Phasen ab Alter (z. B. Ansparphase ab Start, Rentenphase ab 67). Pro Phase: Steuer-Profil, Strategie-Profil und optionale Zielallokation."),
        ["LifecyclePhases.StartAge"] = ("Startalter", "Alter in Jahren, ab dem diese Phase aktiv ist. 0 = von Simulationsstart an. Die Engine wählt pro Monat die Phase anhand des Alters."),
        ["LifecyclePhases.TaxProfileId"] = ("Steuer-Profil", "Welches Steuer-Profil in dieser Phase gilt (Kapitalertragsteuer, Freibetrag, Einkommensteuer für Rente)."),
        ["LifecyclePhases.StrategyProfileId"] = ("Strategie-Profil", "Welches Strategie-Profil in dieser Phase gilt (Liquiditätsreserve, Rebalancing, Lookahead)."),
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
