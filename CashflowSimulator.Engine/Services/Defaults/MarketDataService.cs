using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Standard-Marktfaktoren aus deutscher Sicht: Inflation (VPI), Aktien Welt (ETF),
/// Geldmarkt/Anleihen, Schwellenländer. Inkl. plausible Korrelationen.
/// </summary>
public sealed class MarketDataService : IMarketDataService
{
    /// <inheritdoc />
    public List<EconomicFactorDto> GetEconomicFactors()
    {
        return
        [
            new EconomicFactorDto
            {
                Id = "Inflation_VPI",
                Name = "Inflation (VPI)",
                ModelType = StochasticModelType.OrnsteinUhlenbeck,
                ExpectedReturn = 0.02,
                Volatility = 0.01,
                MeanReversionSpeed = 0.3,
                InitialValue = 0.02
            },
            new EconomicFactorDto
            {
                Id = "Aktien_Welt",
                Name = "Aktien Welt (ETF)",
                ModelType = StochasticModelType.GeometricBrownianMotion,
                ExpectedReturn = 0.07,
                Volatility = 0.15,
                MeanReversionSpeed = 0,
                InitialValue = 100
            },
            new EconomicFactorDto
            {
                Id = "Geldmarkt_Anleihen",
                Name = "Geldmarkt / Anleihen",
                ModelType = StochasticModelType.OrnsteinUhlenbeck,
                ExpectedReturn = 0.02,
                Volatility = 0.03,
                MeanReversionSpeed = 0.2,
                InitialValue = 100
            },
            new EconomicFactorDto
            {
                Id = "Schwellenlaender",
                Name = "Schwellenländer (Aktien)",
                ModelType = StochasticModelType.GeometricBrownianMotion,
                ExpectedReturn = 0.065,
                Volatility = 0.18,
                MeanReversionSpeed = 0,
                InitialValue = 100
            }
        ];
    }

    /// <inheritdoc />
    public List<CorrelationEntryDto> GetCorrelations(List<EconomicFactorDto> factors)
    {
        var idToIndex = factors.Select((f, i) => (f.Id, i)).ToDictionary(x => x.Id, x => x.i);
        if (idToIndex.Count < 2)
            return [];

        var list = new List<CorrelationEntryDto>();
        if (idToIndex.TryGetValue("Inflation_VPI", out var iInf) && idToIndex.TryGetValue("Aktien_Welt", out var iAkt))
            list.Add(new CorrelationEntryDto { FactorIdA = factors[iInf].Id, FactorIdB = factors[iAkt].Id, Correlation = 0.2 });
        if (idToIndex.TryGetValue("Aktien_Welt", out var iA) && idToIndex.TryGetValue("Geldmarkt_Anleihen", out var iB))
            list.Add(new CorrelationEntryDto { FactorIdA = factors[iA].Id, FactorIdB = factors[iB].Id, Correlation = -0.15 });
        if (idToIndex.TryGetValue("Aktien_Welt", out var iAw) && idToIndex.TryGetValue("Schwellenlaender", out var iEm))
            list.Add(new CorrelationEntryDto { FactorIdA = factors[iAw].Id, FactorIdB = factors[iEm].Id, Correlation = 0.75 });
        return list;
    }
}
