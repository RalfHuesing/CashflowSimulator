namespace CashflowSimulator.Desktop.Features.Analysis;

/// <summary>
/// Aggregierte Kennzahlen eines Simulationsjahres (für Master-Liste im Plausibilitäts-Journal).
/// </summary>
public sealed class YearlySummaryItem
{
    /// <summary>Simulationsjahr (0-basiert: Jahr 0, 1, 2, …).</summary>
    public int YearIndex { get; init; }

    /// <summary>Liquidität am Ende des Jahres (letzter Monat des Jahres).</summary>
    public decimal EndCashBalance { get; init; }

    /// <summary>Gesamtvermögen am Ende des Jahres (letzter Monat des Jahres).</summary>
    public decimal EndTotalAssets { get; init; }
}
