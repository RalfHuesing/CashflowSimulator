using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Laufzeit-Zustand pro Monat (Slice 1: Cash + Portfolio; erweiterbar um TaxContext etc.).
/// Wird zu Monatsbeginn gelesen und nach den Processors (z. B. Cashflow, Growth) aktualisiert.
/// </summary>
public sealed class SimulationState
{
    /// <summary>Liquidität (Bargeld) am Monatsende.</summary>
    public decimal Cash { get; set; }

    /// <summary>Gesamtvermögen am Monatsende (Cash + Depot).</summary>
    public decimal TotalAssets { get; set; }

    /// <summary>Depot-Zustand über die Monate (zu Beginn aus Projekt geklont, von GrowthProcessor aktualisiert).</summary>
    public PortfolioDto Portfolio { get; set; } = new();

    /// <summary>Cashflow-Snapshots des aktuellen Monats; nach Übertrag ins MonthlyResultDto leeren.</summary>
    public List<CashflowSnapshotEntryDto> CurrentMonthSnapshots { get; } = [];
}
