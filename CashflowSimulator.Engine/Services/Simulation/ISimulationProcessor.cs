using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Ein Prozessor in der monatlichen Simulations-Pipeline (z. B. Cashflow, Wachstum).
/// Wird pro Monat in fester Reihenfolge aufgerufen.
/// </summary>
public interface ISimulationProcessor
{
    /// <summary>
    /// Verarbeitet einen Simulationsmonat: liest und aktualisiert <paramref name="state"/> anhand von <paramref name="project"/> und <paramref name="currentDate"/>.
    /// </summary>
    void ProcessMonth(SimulationProjectDto project, SimulationState state, DateOnly currentDate);
}
