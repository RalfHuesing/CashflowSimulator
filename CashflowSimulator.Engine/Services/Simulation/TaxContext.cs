using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Steuerkontext für die Laufzeit der Simulation. Enthält die Verlusttöpfe (allgemein und Aktien).
/// Beim Initialisieren werden die Startwerte aus <see cref="SimulationParametersDto"/> geladen;
/// Gewinne im ersten Jahr sind zuerst gegen diese Töpfe zu verrechnen.
/// </summary>
public sealed class TaxContext
{
    /// <summary>Allgemeiner Verlusttopf (nur mit sonstigen Gewinnen verrechenbar).</summary>
    public decimal LossCarryforwardGeneral { get; set; }

    /// <summary>Aktienverlusttopf (nur mit Aktiengewinnen verrechenbar).</summary>
    public decimal LossCarryforwardStocks { get; set; }

    /// <summary>
    /// Erstellt den Kontext und lädt die Verlustvorträge aus den globalen Parametern (Initial State).
    /// </summary>
    public TaxContext(SimulationParametersDto parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        LossCarryforwardGeneral = parameters.InitialLossCarryforwardGeneral;
        LossCarryforwardStocks = parameters.InitialLossCarryforwardStocks;
    }

    /// <summary>
    /// Verrechnet einen sonstigen Gewinn mit dem allgemeinen Verlusttopf. Der Topf wird um den Gewinn reduziert (Minimum 0).
    /// Wird von der Steuerlogik aufgerufen (z. B. bei Jahresabschluss).
    /// </summary>
    public void ApplyGeneralGain(decimal gain)
    {
        if (gain <= 0) return;
        LossCarryforwardGeneral = Math.Max(0, LossCarryforwardGeneral - gain);
    }

    /// <summary>
    /// Verrechnet einen Aktiengewinn mit dem Aktienverlusttopf. Der Topf wird um den Gewinn reduziert (Minimum 0).
    /// Wird von der Steuerlogik aufgerufen (z. B. bei Jahresabschluss).
    /// </summary>
    public void ApplyStocksGain(decimal gain)
    {
        if (gain <= 0) return;
        LossCarryforwardStocks = Math.Max(0, LossCarryforwardStocks - gain);
    }
}
