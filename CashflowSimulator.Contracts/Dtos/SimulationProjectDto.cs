namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Root-DTO eines Simulationsprojekts (Szenario). Wird als JSON geladen/gespeichert.
/// </summary>
public record SimulationProjectDto
{
    public MetaDto Meta { get; init; } = new();
    public SimulationParametersDto Parameters { get; init; } = new();
    public List<CashflowStreamDto> Streams { get; init; } = [];
    public List<CashflowEventDto> Events { get; init; } = [];
    public UiSettingsDto UiSettings { get; init; } = new();
}
