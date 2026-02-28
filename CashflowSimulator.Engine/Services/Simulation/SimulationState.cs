namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Laufzeit-Zustand pro Monat (Slice 1: nur Cash; später erweiterbar um Depotwert, TaxContext etc.).
/// Wird zu Monatsbeginn gelesen und nach den Processors (z. B. Cashflow) aktualisiert.
/// </summary>
public sealed class SimulationState
{
    /// <summary>Liquidität (Bargeld) am Monatsende.</summary>
    public decimal Cash { get; set; }

    /// <summary>Gesamtvermögen am Monatsende. In Slice 1 identisch mit <see cref="Cash"/> (kein Depot).</summary>
    public decimal TotalAssets { get; set; }
}
