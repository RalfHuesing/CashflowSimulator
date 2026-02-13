namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Metadaten eines Simulationsszenarios (reduziert).
/// </summary>
public record MetaDto
{
    public string ScenarioName { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
