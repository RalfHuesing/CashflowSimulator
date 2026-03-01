namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Ergebnis von <see cref="Interfaces.ISimulationResultRepository.StartRunAsync"/>:
/// Run-Id und optional der Pfad zum Ergebnisordner (z. B. Drafts-Ordner mit simulation.db).
/// </summary>
public record RunStartResult(long RunId, string? ResultFolderPath);
