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

    /// <summary>
    /// Definition der ökonomischen Faktoren (Stochastic Factors), die die Markt-/Preispfade steuern
    /// (z. B. MSCI World, Inflation, Gold). Leer = keine stochastischen Faktoren (Engine kann Fallback nutzen).
    /// </summary>
    public List<EconomicFactorDto> EconomicFactors { get; init; } = [];

    /// <summary>
    /// Paarweise Pearson-Korrelationen zwischen Faktoren. Nur Einträge, bei denen die Korrelation
    /// von der impliziten 0 (bzw. 1 auf der Diagonale) abweicht, müssen angegeben werden.
    /// Die Engine baut daraus die volle Korrelationsmatrix und wendet die Cholesky-Zerlegung an.
    /// </summary>
    public List<CorrelationEntryDto> Correlations { get; init; } = [];

    /// <summary>
    /// Master-Zufallsseed für die Monte-Carlo-Simulation. Bei gleichem Seed und gleichen Parametern
    /// liefert die Engine reproduzierbare Pfade (Seed-Replay). 0 = nicht gesetzt (Engine wählt z. B. zeitbasiert).
    /// </summary>
    public int RandomSeed { get; init; }

    /// <summary>
    /// Anzahl der Monte-Carlo-Iterationen (Pfade) pro Simulation. Höhere Werte erhöhen die Genauigkeit
    /// der Verteilungsstatistiken (z. B. Perzentile), kosten aber mehr Rechenzeit.
    /// </summary>
    public int MonteCarloIterations { get; init; }
}
