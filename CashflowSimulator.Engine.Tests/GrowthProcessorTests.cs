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
        var project = ProjectWithFactor(expectedReturn: 0.12); // 12 % p.a., stetige Verzinsung: e^(0.12/12) pro Monat
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

        var expectedPrice = 100m * (decimal)Math.Exp(0.12 / 12.0);
        Assert.Single(state.Portfolio.Assets);
        var asset = state.Portfolio.Assets[0];
        Assert.True(Math.Abs(asset.CurrentPrice - expectedPrice) < 0.0001m);
        Assert.True(Math.Abs((asset.CurrentValue ?? 0) - expectedPrice * 10m) < 0.001m);
        Assert.True(Math.Abs(state.TotalAssets - expectedPrice * 10m) < 0.001m);
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

        var expectedAfter1 = 100m * (decimal)Math.Exp(0.12 / 12.0);
        var expectedAfter2 = 100m * (decimal)Math.Exp(0.12 * 2 / 12.0);
        Assert.True(Math.Abs(afterMonth1 - expectedAfter1) < 0.0001m, $"After month 1: expected ~{expectedAfter1}, got {afterMonth1}");
        Assert.True(Math.Abs(afterMonth2 - expectedAfter2) < 0.0001m, $"After month 2: expected ~{expectedAfter2}, got {afterMonth2}");
    }

    [Fact]
    public void ProcessMonth_TwelveMonths_MatchesAnnualContinuousGrowth()
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

        for (var month = 1; month <= 12; month++)
            processor.ProcessMonth(project, state, new DateOnly(2020, month, 1));

        var expectedAfter12Months = 100m * (decimal)Math.Exp(0.12);
        Assert.True(Math.Abs(state.Portfolio.Assets[0].CurrentPrice - expectedAfter12Months) < 0.0001m);
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

    [Fact]
    public void ProcessMonth_AssetWithTranches_LeavesTranchesUnchanged()
    {
        var processor = new GrowthProcessor();
        var project = ProjectWithFactor(expectedReturn: 0.12);
        var tranches = new List<AssetTrancheDto>
        {
            new() { PurchaseDate = new DateOnly(2020, 1, 1), Quantity = 3m, AcquisitionPricePerUnit = 95m },
            new() { PurchaseDate = new DateOnly(2020, 2, 1), Quantity = 2m, AcquisitionPricePerUnit = 105m }
        };
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
                        CurrentQuantity = 5m,
                        CurrentValue = 500m,
                        Tranches = tranches
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 3, 1));

        var expectedPrice = 100m * (decimal)Math.Exp(0.12 / 12.0);
        var asset = state.Portfolio.Assets[0];
        Assert.True(Math.Abs(asset.CurrentPrice - expectedPrice) < 0.0001m);
        Assert.True(Math.Abs((asset.CurrentValue ?? 0) - expectedPrice * 5m) < 0.001m);
        Assert.Equal(2, asset.Tranches.Count);
        Assert.Equal(new DateOnly(2020, 1, 1), asset.Tranches[0].PurchaseDate);
        Assert.Equal(3m, asset.Tranches[0].Quantity);
        Assert.Equal(95m, asset.Tranches[0].AcquisitionPricePerUnit);
        Assert.Equal(new DateOnly(2020, 2, 1), asset.Tranches[1].PurchaseDate);
        Assert.Equal(2m, asset.Tranches[1].Quantity);
        Assert.Equal(105m, asset.Tranches[1].AcquisitionPricePerUnit);
    }
}
