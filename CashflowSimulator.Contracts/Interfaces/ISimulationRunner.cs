using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Führt eine monatliche Simulation über den Zeithorizont des Projekts aus (Clean Architecture: Engine-Implementierung, Desktop ruft Interface auf).
/// </summary>
public interface ISimulationRunner
{
    /// <summary>
    /// Führt die Simulation für das gegebene Projekt asynchron aus und liefert die Run-Id (Monatsdaten im Repository).
    /// </summary>
    /// <param name="project">Projekt mit Parametern und Cashflow-Streams.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Ergebnis mit RunId; monatliche Details per GetMonthlyResultsAsync am Repository/ResultAnalysisService.</returns>
    Task<SimulationResultDto> RunSimulationAsync(SimulationProjectDto project, CancellationToken cancellationToken = default);
}
