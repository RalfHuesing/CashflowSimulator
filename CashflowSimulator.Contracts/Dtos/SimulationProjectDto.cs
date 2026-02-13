namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Root-DTO eines Simulationsprojekts (Szenario). Wird als JSON geladen/gespeichert.
/// </summary>
public record SimulationProjectDto
{
    public MetaDto Meta { get; init; } = new();
    public SimulationParametersDto Parameters { get; init; } = new();
}
