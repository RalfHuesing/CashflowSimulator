using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Schreibt einen Snapshot des Eingabe-Szenarios (SimulationProjectDto) in einen Ergebnisordner, z. B. als input_scenario.json.
/// Wird vom SimulationRunner nach Abschluss eines Laufs aufgerufen.
/// </summary>
public interface IScenarioSnapshotWriter
{
    /// <summary>
    /// Schreibt das Projekt als JSON in den angegebenen Ordner (z. B. input_scenario.json).
    /// </summary>
    /// <param name="resultFolderPath">Absoluter Pfad zum Ergebnisordner (muss existieren).</param>
    /// <param name="project">Das Szenario-Projekt zum Zeitpunkt des Laufs.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    Task WriteAsync(string resultFolderPath, SimulationProjectDto project, CancellationToken cancellationToken = default);
}
