using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Orchestriert die Default-Daten-Services und baut daraus ein erweitertes Standard-Szenario
/// für einen durchschnittlichen deutschen Single-Haushalt.
/// Enthält zwei Lifecycle-Phasen: Ansparphase (ab aktuellem Alter) und Rentenphase (ab 67).
/// </summary>
public sealed class DefaultProjectProvider(
    ISimulationTimeService simulationTimeService,
    IMarketDataService marketDataService,
    ICashflowDefaultService cashflowDefaultService,
    IPortfolioDefaultService portfolioDefaultService) : IDefaultProjectProvider
{
    /// <inheritdoc />
    public SimulationProjectDto CreateDefault()
    {
        var timeContext = simulationTimeService.GetDefaultTimeContext();
        var parameters = timeContext.Parameters;
        var simulationStart = parameters.SimulationStart;
        var simulationEnd = parameters.SimulationEnd;

        List<CashflowStreamDto> streams = cashflowDefaultService.GetStreams(simulationStart, simulationEnd);
        List<CashflowEventDto> events = cashflowDefaultService.GetEvents(simulationStart, simulationEnd);
        List<EconomicFactorDto> economicFactors = marketDataService.GetEconomicFactors();
        List<CorrelationEntryDto> correlations = marketDataService.GetCorrelations(economicFactors);
        List<AssetClassDto> assetClasses = portfolioDefaultService.GetAssetClasses();
        PortfolioDto portfolio = portfolioDefaultService.GetPortfolio(simulationStart, assetClasses);

        var (taxProfiles, strategyProfiles, allocationProfiles, lifecyclePhases) =
            DefaultLifecycleDataBuilder.Build(parameters, assetClasses);

        return new SimulationProjectDto
        {
            Meta = new MetaDto
            {
                ScenarioName = "Beispiel: Deutscher Single (30 Jahre)",
                CreatedAt = timeContext.Now
            },
            Parameters = parameters,
            Streams = streams,
            Events = events,
            EconomicFactors = economicFactors,
            Correlations = correlations,
            AssetClasses = assetClasses,
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            AllocationProfiles = allocationProfiles,
            LifecyclePhases = lifecyclePhases,
            Portfolio = portfolio,
            UiSettings = new UiSettingsDto()
        };
    }
}
