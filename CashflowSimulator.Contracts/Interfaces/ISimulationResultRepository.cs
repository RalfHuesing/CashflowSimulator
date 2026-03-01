using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Persistenz für Simulationsergebnisse (SQLite). Ein Run = ein Simulationslauf.
/// Async Write-Stream: StartRunAsync, WriteMonthlyResultsAsync (Batch), CompleteRunAsync. Read: GetMonthlyResultsAsync.
/// </summary>
public interface ISimulationResultRepository
{
    /// <summary>
    /// Bereitet einen neuen Run vor (leere DB pro Run), räumt zuvor das Temp-Verzeichnis auf (nur eigene Dateien).
    /// </summary>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Run-Id des neuen Laufs.</returns>
    Task<long> StartRunAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Schreibt alle Monatsergebnisse inkl. Snapshots für den angegebenen Run in einer Transaktion (Batch).
    /// </summary>
    /// <param name="runId">Run-Id.</param>
    /// <param name="entries">Monatsergebnisse in Reihenfolge.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    Task WriteMonthlyResultsAsync(long runId, IEnumerable<MonthlyResultDto> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Markiert den Run als abgeschlossen (z. B. Connection schließen).
    /// </summary>
    Task CompleteRunAsync(long runId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liest alle Monatsergebnisse eines Runs in Reihenfolge.
    /// </summary>
    /// <param name="runId">Run-Id.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    Task<IReadOnlyList<MonthlyResultDto>> GetMonthlyResultsAsync(long runId, CancellationToken cancellationToken = default);
}
