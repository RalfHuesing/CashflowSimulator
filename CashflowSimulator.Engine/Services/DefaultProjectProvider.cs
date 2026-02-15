using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Engine.Services.Defaults;

namespace CashflowSimulator.Engine.Services;

/// <summary>
/// Orchestriert die Default-Daten-Services und baut daraus ein erweitertes Standard-Szenario
/// für einen durchschnittlichen deutschen Single-Haushalt.
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
        var retirementDate = parameters.RetirementDate;

        List<CashflowStreamDto> streams = cashflowDefaultService.GetStreams(simulationStart, retirementDate);
        List<CashflowEventDto> events = cashflowDefaultService.GetEvents(simulationStart, retirementDate);
        List<EconomicFactorDto> economicFactors = marketDataService.GetEconomicFactors();
        List<CorrelationEntryDto> correlations = marketDataService.GetCorrelations(economicFactors);
        List<AssetClassDto> assetClasses = portfolioDefaultService.GetAssetClasses();
        PortfolioDto portfolio = portfolioDefaultService.GetPortfolio(simulationStart, assetClasses);

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
            Portfolio = portfolio,
            UiSettings = new UiSettingsDto()
        };
    }
}
