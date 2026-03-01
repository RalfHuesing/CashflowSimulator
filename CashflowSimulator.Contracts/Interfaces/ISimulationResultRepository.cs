using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Persistenz für Simulationsergebnisse (SQLite). Ein Run = ein Simulationslauf.
/// Write-Stream: StartRun, monatlich WriteMonthlyResult, CompleteRun. Read: GetMonthlyResults pro Run.
/// </summary>
public interface ISimulationResultRepository
{
    /// <summary>
    /// Bereitet einen neuen Run vor (leere DB pro Run), räumt zuvor das Temp-Verzeichnis auf (nur eigene Dateien).
    /// </summary>
    /// <returns>Run-Id des neuen Laufs.</returns>
    long StartRun();

    /// <summary>
    /// Schreibt ein Monatsergebnis inkl. Snapshots für den angegebenen Run.
    /// </summary>
    void WriteMonthlyResult(long runId, MonthlyResultDto entry);

    /// <summary>
    /// Markiert den Run als abgeschlossen (z. B. Connection schließen).
    /// </summary>
    void CompleteRun(long runId);

    /// <summary>
    /// Liest alle Monatsergebnisse eines Runs in Reihenfolge.
    /// </summary>
    IReadOnlyList<MonthlyResultDto> GetMonthlyResults(long runId);
}
