using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Führt eine monatliche Simulation über den Zeithorizont des Projekts aus (Clean Architecture: Engine-Implementierung, Desktop ruft Interface auf).
/// </summary>
public interface ISimulationRunner
{
    /// <summary>
    /// Führt die Simulation für das gegebene Projekt aus und liefert die monatlichen Ergebnisse.
    /// </summary>
    /// <param name="project">Projekt mit Parametern und Cashflow-Streams.</param>
    /// <returns>Ergebnis mit Liste der monatlichen Ergebnisse (Slice 1: nur Cashflow, kein Depot/Steuern).</returns>
    SimulationResultDto RunSimulation(SimulationProjectDto project);
}
