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
        LossCarryforwardGeneral = parameters.InitialLossCarryforwardGeneral;
        LossCarryforwardStocks = parameters.InitialLossCarryforwardStocks;
    }
}
