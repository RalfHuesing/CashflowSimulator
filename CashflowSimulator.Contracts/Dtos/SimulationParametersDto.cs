namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Zeithorizont und Stammparameter der Simulation.
/// </summary>
public record SimulationParametersDto
{
    public int StartYear { get; init; }
    public int EndYear { get; init; }
    public int BirthYear { get; init; }
    public int RetirementYear { get; init; }
    public decimal InitialLiquidCash { get; init; }
    public string CurrencyCode { get; init; } = "EUR";
}
