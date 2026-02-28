namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Gesamtergebnis einer Simulation: monatliche Ergebnisse von Start bis Ende.
/// </summary>
public record SimulationResultDto
{
    /// <summary>Monatliche Ergebnisse in Reihenfolge (Monat 0 bis N-1).</summary>
    public List<MonthlyResultDto> MonthlyResults { get; init; } = [];
}
