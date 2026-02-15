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
    /// Anlageklassen (Buckets) für die strategische Zielallokation (z. B. "Aktien Welt 70 %", "Anleihen 30 %").
    /// Vermögenswerte werden über <see cref="AssetDto.AssetClassId"/> einer Klasse zugeordnet.
    /// </summary>
    public List<AssetClassDto> AssetClasses { get; init; } = [];

    /// <summary>
    /// Steuer-Profile (z. B. Standard, Rentenbesteuerung). Werden von <see cref="LifecyclePhases"/> referenziert.
    /// </summary>
    public List<TaxProfileDto> TaxProfiles { get; init; } = [];

    /// <summary>
    /// Strategie-Profile (z. B. Aufbau, Entnahme). Werden von <see cref="LifecyclePhases"/> referenziert.
    /// </summary>
    public List<StrategyProfileDto> StrategyProfiles { get; init; } = [];

    /// <summary>
    /// Lebensphasen (z. B. Ansparphase ab Start, Rentenphase ab 67). Pro Phase: Steuer- und Strategie-Profil.
    /// Die Engine wählt pro Monat die Phase anhand des Alters (aus <see cref="SimulationParametersDto.DateOfBirth"/>).
    /// </summary>
    public List<LifecyclePhaseDto> LifecyclePhases { get; init; } = [];

    /// <summary>
    /// Portfolio des Nutzers: alle gehaltenen Vermögenswerte (Assets) und optional Strategie/Rebalancing.
    /// Jedes Asset verknüpft sich über <see cref="AssetDto.EconomicFactorId"/> mit einem
    /// <see cref="EconomicFactorDto"/> aus <see cref="EconomicFactors"/> für die Wertentwicklung.
    /// </summary>
    public PortfolioDto Portfolio { get; init; } = new();

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
