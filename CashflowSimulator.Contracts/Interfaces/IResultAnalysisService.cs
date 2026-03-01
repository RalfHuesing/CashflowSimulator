using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Service für die Abfrage von Simulationsergebnissen (z. B. für das Result-ViewModel).
/// </summary>
public interface IResultAnalysisService
{
    /// <summary>
    /// Liefert die monatlichen Ergebnisse eines Runs in Reihenfolge.
    /// </summary>
    IReadOnlyList<MonthlyResultDto> GetMonthlyResults(long runId);
}
