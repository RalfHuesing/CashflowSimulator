using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Validation.Tests.TestData;

/// <summary>
/// Builder für gültige und ungültige <see cref="SimulationProjectDto"/>-Instanzen in Tests.
/// Zentralisiert Lifecycle-Setup (Tax/Strategy/Phase) und reduziert Duplikate in Validator- und Service-Tests.
/// </summary>
public sealed class SimulationProjectDtoBuilder
{
    private MetaDto? _meta;
    private SimulationParametersDto? _parameters;
    private List<TaxProfileDto>? _taxProfiles;
    private List<StrategyProfileDto>? _strategyProfiles;
    private List<LifecyclePhaseDto>? _lifecyclePhases;
    private UiSettingsDto? _uiSettings;
    private List<EconomicFactorDto>? _economicFactors;
    private List<CorrelationEntryDto>? _correlations;
    private List<CashflowStreamDto>? _streams;
    private List<CashflowEventDto>? _events;
    private PortfolioDto? _portfolio;

    /// <summary>
    /// Setzt Meta (z. B. für ungültigen Szenario-Namen).
    /// </summary>
    public SimulationProjectDtoBuilder WithMeta(MetaDto value)
    {
        _meta = value;
        return this;
    }

    /// <summary>
    /// Setzt Parameters (z. B. für ungültiges Simulationsende).
    /// </summary>
    public SimulationProjectDtoBuilder WithParameters(SimulationParametersDto value)
    {
        _parameters = value;
        return this;
    }

    /// <summary>
    /// Setzt eine gültige Lifecycle-Kombination: eine Tax-, eine Strategy- und eine Phase mit StartAge 0.
    /// </summary>
    public SimulationProjectDtoBuilder WithValidLifecycle()
    {
        var taxId = Guid.NewGuid().ToString();
        var strategyId = Guid.NewGuid().ToString();
        _taxProfiles =
        [
            new TaxProfileDto
            {
                Id = taxId,
                Name = "Standard",
                CapitalGainsTaxRate = 0.26375m,
                TaxFreeAllowance = 1000m,
                IncomeTaxRate = 0.35m
            }
        ];
        _strategyProfiles =
        [
            new StrategyProfileDto
            {
                Id = strategyId,
                Name = "Aufbau",
                CashReserveMonths = 3,
                RebalancingThreshold = 0.05m,
                MinimumTransactionAmount = 50m,
                LookaheadMonths = 24
            }
        ];
        _lifecyclePhases =
        [
            new LifecyclePhaseDto
            {
                StartAge = 0,
                TaxProfileId = taxId,
                StrategyProfileId = strategyId,
                AssetAllocationOverrides = []
            }
        ];
        return this;
    }

    /// <summary>
    /// Setzt leere Lifecycle-Phasen (ungültig: keine Phase deckt Simulationsstart ab).
    /// </summary>
    public SimulationProjectDtoBuilder WithEmptyLifecyclePhases()
    {
        WithValidLifecycle();
        _lifecyclePhases = [];
        return this;
    }

    /// <summary>
    /// Setzt Lifecycle so, dass keine Phase den Simulationsstart abdeckt (Phase nur ab 67, Alter zum Start 30).
    /// </summary>
    public SimulationProjectDtoBuilder WithNoPhaseCoveringSimulationStart()
    {
        var taxId = Guid.NewGuid().ToString();
        var strategyId = Guid.NewGuid().ToString();
        _parameters ??= new SimulationParametersDtoBuilder()
            .WithDateOfBirth(new DateOnly(1990, 1, 1))
            .WithSimulationEnd(new DateOnly(2085, 1, 1))
            .Build();
        _parameters = _parameters with { SimulationStart = new DateOnly(2020, 1, 1) };
        _taxProfiles =
        [
            new TaxProfileDto
            {
                Id = taxId,
                Name = "Rente",
                CapitalGainsTaxRate = 0.26m,
                TaxFreeAllowance = 1000m,
                IncomeTaxRate = 0.18m
            }
        ];
        _strategyProfiles =
        [
            new StrategyProfileDto
            {
                Id = strategyId,
                Name = "Entnahme",
                CashReserveMonths = 12,
                RebalancingThreshold = 0.05m,
                LookaheadMonths = 6
            }
        ];
        _lifecyclePhases =
        [
            new LifecyclePhaseDto
            {
                StartAge = 67,
                TaxProfileId = taxId,
                StrategyProfileId = strategyId,
                AssetAllocationOverrides = []
            }
        ];
        return this;
    }

    /// <summary>
    /// Setzt eine Phase, die auf eine nicht existierende TaxProfileId verweist.
    /// </summary>
    public SimulationProjectDtoBuilder WithPhaseReferencingNonExistentTaxProfile(string nonExistentTaxId = "non-existent-id")
    {
        var strategyId = Guid.NewGuid().ToString();
        _taxProfiles =
        [
            new TaxProfileDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Standard",
                CapitalGainsTaxRate = 0.26m,
                TaxFreeAllowance = 1000m,
                IncomeTaxRate = 0.35m
            }
        ];
        _strategyProfiles =
        [
            new StrategyProfileDto
            {
                Id = strategyId,
                Name = "Aufbau",
                CashReserveMonths = 3,
                RebalancingThreshold = 0.05m,
                LookaheadMonths = 24
            }
        ];
        _lifecyclePhases =
        [
            new LifecyclePhaseDto
            {
                StartAge = 0,
                TaxProfileId = nonExistentTaxId,
                StrategyProfileId = strategyId,
                AssetAllocationOverrides = []
            }
        ];
        return this;
    }

    /// <summary>
    /// Setzt UiSettings (null für Ungültig-Tests).
    /// </summary>
    public SimulationProjectDtoBuilder WithUiSettings(UiSettingsDto? value)
    {
        _uiSettings = value;
        return this;
    }

    /// <summary>
    /// Setzt EconomicFactors und optional Correlations (für Korrelationsmatrix-Tests).
    /// </summary>
    public SimulationProjectDtoBuilder WithEconomicFactors(
        List<EconomicFactorDto> factors,
        List<CorrelationEntryDto>? correlations = null)
    {
        _economicFactors = factors;
        _correlations = correlations;
        return this;
    }

    /// <summary>
    /// Setzt Streams (z. B. für CurrentProjectService-Tests).
    /// </summary>
    public SimulationProjectDtoBuilder WithStreams(List<CashflowStreamDto> streams)
    {
        _streams = streams;
        return this;
    }

    /// <summary>
    /// Setzt Events (z. B. für CurrentProjectService-Tests).
    /// </summary>
    public SimulationProjectDtoBuilder WithEvents(List<CashflowEventDto> events)
    {
        _events = events;
        return this;
    }

    /// <summary>
    /// Setzt Portfolio (z. B. für Asset-EconomicFactorId-Validierung).
    /// </summary>
    public SimulationProjectDtoBuilder WithPortfolio(PortfolioDto portfolio)
    {
        _portfolio = portfolio;
        return this;
    }

    /// <summary>
    /// Erstellt ein <see cref="SimulationProjectDto"/> mit gesetzten oder Standardwerten.
    /// Wenn Lifecycle nicht gesetzt wurde, wird WithValidLifecycle() implizit verwendet.
    /// </summary>
    public SimulationProjectDto Build()
    {
        if (_taxProfiles is null || _strategyProfiles is null || _lifecyclePhases is null)
            WithValidLifecycle();

        return new SimulationProjectDto
        {
            Meta = _meta ?? new MetaDtoBuilder().Build(),
            Parameters = _parameters ?? new SimulationParametersDtoBuilder().Build(),
            TaxProfiles = _taxProfiles!,
            StrategyProfiles = _strategyProfiles!,
            LifecyclePhases = _lifecyclePhases!,
            UiSettings = _uiSettings ?? new UiSettingsDto(),
            Streams = _streams ?? [],
            Events = _events ?? [],
            EconomicFactors = _economicFactors ?? [],
            Correlations = _correlations ?? [],
            AssetClasses = [],
            Portfolio = _portfolio ?? new PortfolioDto()
        };
    }
}
