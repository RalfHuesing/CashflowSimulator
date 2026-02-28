using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

public sealed class GrowthProcessorTests
{
    private const string FactorId = "MSCI_World";

    private static SimulationProjectDto ProjectWithFactor(double expectedReturn = 0.06)
    {
        return new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto(),
            Streams = [],
            EconomicFactors =
            [
                new EconomicFactorDto
                {
                    Id = FactorId,
                    Name = "MSCI World",
                    ModelType = StochasticModelType.GeometricBrownianMotion,
                    ExpectedReturn = expectedReturn,
                    Volatility = 0.18,
                    InitialValue = 100.0
                }
            ]
        };
    }

    [Fact]
    public void ProcessMonth_NoAssets_SetsTotalAssetsToCash()
    {
        var processor = new GrowthProcessor();
        var project = ProjectWithFactor();
        var state = new SimulationState
        {
            Cash = 10_000m,
            Portfolio = new PortfolioDto { Assets = [] }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        Assert.Equal(10_000m, state.TotalAssets);
        Assert.Empty(state.Portfolio.Assets);
    }

    [Fact]
    public void ProcessMonth_OneAsset_AppliesDeterministicGrowth()
    {
        var processor = new GrowthProcessor();
        var project = ProjectWithFactor(expectedReturn: 0.12); // 12 % p.a. => 1 % pro Monat
        var state = new SimulationState
        {
            Cash = 0m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        Name = "ETF",
                        EconomicFactorId = FactorId,
                        CurrentPrice = 100m,
                        CurrentQuantity = 10m,
                        CurrentValue = 1000m
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        // newPrice = 100 * (1 + 0.12/12) = 101
        Assert.Single(state.Portfolio.Assets);
        var asset = state.Portfolio.Assets[0];
        Assert.Equal(101m, asset.CurrentPrice);
        Assert.Equal(1010m, asset.CurrentValue);
        Assert.Equal(1010m, state.TotalAssets);
    }

    [Fact]
    public void ProcessMonth_TwoMonths_CompoundGrowth()
    {
        var processor = new GrowthProcessor();
        var project = ProjectWithFactor(expectedReturn: 0.12);
        var state = new SimulationState
        {
            Cash = 0m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        Name = "ETF",
                        EconomicFactorId = FactorId,
                        CurrentPrice = 100m,
                        CurrentQuantity = 1m,
                        CurrentValue = 100m
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        var afterMonth1 = state.Portfolio.Assets[0].CurrentPrice;
        processor.ProcessMonth(project, state, new DateOnly(2020, 2, 1));
        var afterMonth2 = state.Portfolio.Assets[0].CurrentPrice;

        Assert.Equal(101m, afterMonth1);
        Assert.True(afterMonth2 > 101m && afterMonth2 < 103m); // 101 * 1.01 ≈ 102.01
    }

    [Fact]
    public void ProcessMonth_AssetWithoutMatchingFactor_LeavesPriceUnchanged()
    {
        var processor = new GrowthProcessor();
        var project = ProjectWithFactor();
        var state = new SimulationState
        {
            Cash = 500m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        Name = "Orphan",
                        EconomicFactorId = "NonExistentId",
                        CurrentPrice = 50m,
                        CurrentQuantity = 2m,
                        CurrentValue = 100m
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        Assert.Single(state.Portfolio.Assets);
        Assert.Equal(50m, state.Portfolio.Assets[0].CurrentPrice);
        Assert.Equal(100m, state.Portfolio.Assets[0].CurrentValue);
        Assert.Equal(600m, state.TotalAssets); // Cash + unchanged asset value
    }

    [Fact]
    public void ProcessMonth_CashAndPortfolio_TotalAssetsIsSum()
    {
        var processor = new GrowthProcessor();
        var project = ProjectWithFactor(expectedReturn: 0);
        var state = new SimulationState
        {
            Cash = 1000m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        Name = "ETF",
                        EconomicFactorId = FactorId,
                        CurrentPrice = 100m,
                        CurrentQuantity = 5m,
                        CurrentValue = 500m
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        Assert.Equal(100m, state.Portfolio.Assets[0].CurrentPrice); // 0 % growth
        Assert.Equal(500m, state.Portfolio.Assets[0].CurrentValue);
        Assert.Equal(1500m, state.TotalAssets);
    }
}
