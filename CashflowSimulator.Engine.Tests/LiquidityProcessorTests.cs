using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

public sealed class LiquidityProcessorTests
{
    private const string ClassA = "Aktien";
    private const string ClassB = "Anleihen";

    private static SimulationProjectDto Project(
        List<LifecyclePhaseDto>? phases = null,
        List<AllocationProfileDto>? profiles = null,
        SimulationParametersDto? parameters = null)
    {
        var profileId = "profile1";
        var defaultProfiles = new List<AllocationProfileDto>
        {
            new()
            {
                Id = profileId,
                Name = "70/30",
                Entries =
                [
                    new AllocationProfileEntryDto { AssetClassId = ClassA, TargetWeight = 0.70m },
                    new AllocationProfileEntryDto { AssetClassId = ClassB, TargetWeight = 0.30m }
                ]
            }
        };
        var defaultPhases = new List<LifecyclePhaseDto>
        {
            new() { StartAge = 0, AllocationProfileId = profileId }
        };
        return new SimulationProjectDto
        {
            Parameters = parameters ?? new SimulationParametersDto
            {
                SimulationStart = new DateOnly(2020, 1, 1),
                SimulationEnd = new DateOnly(2030, 1, 1),
                DateOfBirth = new DateOnly(1990, 1, 1),
                InitialLiquidCash = 0m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            LifecyclePhases = phases ?? defaultPhases,
            AllocationProfiles = profiles ?? defaultProfiles
        };
    }

    [Fact]
    public void ProcessMonth_CashZero_LeavesPortfolioUnchanged()
    {
        var processor = new LiquidityProcessor();
        var project = Project();
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
                        AssetClassId = ClassA,
                        CurrentPrice = 100m,
                        CurrentQuantity = 10m,
                        CurrentValue = 1000m
                    }
                ]
            }
        };
        var snapshot = state.Portfolio.Assets[0].CurrentQuantity;

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(0m, state.Cash);
        Assert.Equal(snapshot, state.Portfolio.Assets[0].CurrentQuantity);
        Assert.Empty(state.Portfolio.Assets[0].Transactions);
    }

    [Fact]
    public void ProcessMonth_CashPositive_NoLifecyclePhases_LeavesCashUnchanged()
    {
        var processor = new LiquidityProcessor();
        var project = Project(phases: []);
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
                        AssetClassId = ClassA,
                        CurrentPrice = 100m,
                        CurrentQuantity = 0m,
                        CurrentValue = 0m
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(1000m, state.Cash);
    }

    [Fact]
    public void ProcessMonth_CashPositive_PhaseWithoutAllocationProfileId_LeavesCashUnchanged()
    {
        var processor = new LiquidityProcessor();
        var project = Project(phases: [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = "" }]);
        var state = new SimulationState
        {
            Cash = 1000m,
            Portfolio = new PortfolioDto { Assets = [] }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(1000m, state.Cash);
    }

    [Fact]
    public void ProcessMonth_CashPositive_OneClassOneActiveAsset_InvestsAndAddsBuyTransaction()
    {
        var processor = new LiquidityProcessor();
        var profileId = "p1";
        var project = Project(
            profiles: [new AllocationProfileDto { Id = profileId, Name = "100% A", Entries = [new AllocationProfileEntryDto { AssetClassId = ClassA, TargetWeight = 1.0m }] }],
            phases: [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = profileId }]);
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
                        Name = "ETF A",
                        AssetClassId = ClassA,
                        CurrentPrice = 100m,
                        CurrentQuantity = 0m,
                        CurrentValue = 0m,
                        IsActiveSavingsInstrument = true
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.True(state.Cash < 500m);
        Assert.Equal(5m, state.Portfolio.Assets[0].CurrentQuantity); // 500/100 = 5
        Assert.Equal(500m, state.Portfolio.Assets[0].CurrentValue);
        Assert.Single(state.Portfolio.Assets[0].Transactions);
        Assert.Equal(TransactionType.Buy, state.Portfolio.Assets[0].Transactions[0].Type);
        Assert.Equal(5m, state.Portfolio.Assets[0].Transactions[0].Quantity);
        Assert.Equal(100m, state.Portfolio.Assets[0].Transactions[0].PricePerUnit);
        Assert.Equal(500m, state.Portfolio.Assets[0].Transactions[0].TotalAmount);
        Assert.Equal(0m, state.Cash); // 500 - 500
        // FIFO: eine neue Tranche angelegt
        Assert.Single(state.Portfolio.Assets[0].Tranches);
        Assert.Equal(new DateOnly(2020, 6, 1), state.Portfolio.Assets[0].Tranches[0].PurchaseDate);
        Assert.Equal(5m, state.Portfolio.Assets[0].Tranches[0].Quantity);
        Assert.Equal(100m, state.Portfolio.Assets[0].Tranches[0].AcquisitionPricePerUnit);
    }

    [Fact]
    public void ProcessMonth_CashPositive_TwoClassesTwoAssets_Invests70_30()
    {
        var processor = new LiquidityProcessor();
        var profileId = "p1";
        var project = Project(
            profiles: [new AllocationProfileDto { Id = profileId, Name = "70/30", Entries = [new AllocationProfileEntryDto { AssetClassId = ClassA, TargetWeight = 0.70m }, new AllocationProfileEntryDto { AssetClassId = ClassB, TargetWeight = 0.30m }] }],
            phases: [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = profileId }]);
        var state = new SimulationState
        {
            Cash = 1000m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto { Id = "a1", AssetClassId = ClassA, CurrentPrice = 100m, CurrentQuantity = 0m, CurrentValue = 0m, IsActiveSavingsInstrument = true },
                    new AssetDto { Id = "a2", AssetClassId = ClassB, CurrentPrice = 100m, CurrentQuantity = 0m, CurrentValue = 0m, IsActiveSavingsInstrument = true }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        // 70% = 700 -> 7 shares, 30% = 300 -> 3 shares
        Assert.Equal(7m, state.Portfolio.Assets[0].CurrentQuantity);
        Assert.Equal(3m, state.Portfolio.Assets[1].CurrentQuantity);
        Assert.Equal(0m, state.Cash); // 1000 fully invested
        Assert.Single(state.Portfolio.Assets[0].Transactions);
        Assert.Single(state.Portfolio.Assets[1].Transactions);
    }

    [Fact]
    public void ProcessMonth_CashNegative_OneAssetWithValue_SellsUntilCashZero()
    {
        var processor = new LiquidityProcessor();
        var project = Project();
        var state = new SimulationState
        {
            Cash = -400m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        AssetClassId = ClassA,
                        CurrentPrice = 100m,
                        CurrentQuantity = 10m,
                        CurrentValue = 1000m
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(0m, state.Cash); // -400 + 400
        Assert.Equal(6m, state.Portfolio.Assets[0].CurrentQuantity); // sold 4
        Assert.Equal(600m, state.Portfolio.Assets[0].CurrentValue);
        Assert.Single(state.Portfolio.Assets[0].Transactions);
        Assert.Equal(TransactionType.Sell, state.Portfolio.Assets[0].Transactions[0].Type);
        Assert.Equal(4m, state.Portfolio.Assets[0].Transactions[0].Quantity);
        Assert.Equal(400m, state.Portfolio.Assets[0].Transactions[0].TotalAmount);
    }

    [Fact]
    public void ProcessMonth_CashNegative_TwoAssets_ProRataSell()
    {
        var processor = new LiquidityProcessor();
        var project = Project();
        var state = new SimulationState
        {
            Cash = -500m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto { Id = "a1", AssetClassId = ClassA, CurrentPrice = 100m, CurrentQuantity = 10m, CurrentValue = 1000m },
                    new AssetDto { Id = "a2", AssetClassId = ClassB, CurrentPrice = 100m, CurrentQuantity = 10m, CurrentValue = 1000m }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        // Pro rata: 250 from each -> Floor(250/100)=2 shares each = 400 proceeds; Cash = -500+400 = -100
        Assert.Equal(-100m, state.Cash);
        Assert.Equal(2m, state.Portfolio.Assets[0].Transactions[0].Quantity);
        Assert.Equal(2m, state.Portfolio.Assets[1].Transactions[0].Quantity);
        Assert.Equal(8m, state.Portfolio.Assets[0].CurrentQuantity);
        Assert.Equal(8m, state.Portfolio.Assets[1].CurrentQuantity);
    }

    [Fact]
    public void ProcessMonth_CashNegative_PortfolioValueLessThanDeficit_CashStaysNegative()
    {
        var processor = new LiquidityProcessor();
        var project = Project();
        var state = new SimulationState
        {
            Cash = -2000m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto { Id = "a1", AssetClassId = ClassA, CurrentPrice = 100m, CurrentQuantity = 10m, CurrentValue = 1000m }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(-1000m, state.Cash); // -2000 + 1000 (sold all 10)
        Assert.Equal(0m, state.Portfolio.Assets[0].CurrentQuantity);
    }

    [Fact]
    public void ProcessMonth_Immutability_OriginalAssetListsNotMutated()
    {
        var processor = new LiquidityProcessor();
        var transactions = new List<TransactionDto>();
        var asset = new AssetDto
        {
            Id = "a1",
            AssetClassId = ClassA,
            CurrentPrice = 100m,
            CurrentQuantity = 10m,
            CurrentValue = 1000m,
            Transactions = transactions
        };
        var assets = new List<AssetDto> { asset };
        var project = Project(phases: [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = "profile1" }]);
        var state = new SimulationState
        {
            Cash = -300m,
            Portfolio = new PortfolioDto { Assets = assets }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Empty(transactions);
        Assert.Equal(10m, asset.CurrentQuantity);
        Assert.Single(state.Portfolio.Assets);
        Assert.Equal(7m, state.Portfolio.Assets[0].CurrentQuantity);
        Assert.NotSame(asset, state.Portfolio.Assets[0]);
    }

    [Fact]
    public void ProcessMonth_CashPositive_TwoMonths_TwoTranches()
    {
        var processor = new LiquidityProcessor();
        var profileId = "p1";
        var project = Project(
            profiles: [new AllocationProfileDto { Id = profileId, Name = "100% A", Entries = [new AllocationProfileEntryDto { AssetClassId = ClassA, TargetWeight = 1.0m }] }],
            phases: [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = profileId }]);
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
                        AssetClassId = ClassA,
                        CurrentPrice = 100m,
                        CurrentQuantity = 0m,
                        CurrentValue = 0m,
                        IsActiveSavingsInstrument = true
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));
        Assert.Single(state.Portfolio.Assets[0].Tranches);
        state.Cash = 300m;
        processor.ProcessMonth(project, state, new DateOnly(2020, 7, 1));

        Assert.Equal(2, state.Portfolio.Assets[0].Tranches.Count);
        Assert.Equal(new DateOnly(2020, 6, 1), state.Portfolio.Assets[0].Tranches[0].PurchaseDate);
        Assert.Equal(new DateOnly(2020, 7, 1), state.Portfolio.Assets[0].Tranches[1].PurchaseDate);
        Assert.Equal(5m + 3m, state.Portfolio.Assets[0].CurrentQuantity);
        Assert.Equal(state.Portfolio.Assets[0].Tranches.Sum(t => t.Quantity), state.Portfolio.Assets[0].CurrentQuantity);
    }

    [Fact]
    public void ProcessMonth_CashNegative_FifoConsumesOldestTranchesFirst()
    {
        var processor = new LiquidityProcessor();
        var project = Project();
        var state = new SimulationState
        {
            Cash = -600m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        AssetClassId = ClassA,
                        CurrentPrice = 100m,
                        CurrentQuantity = 10m,
                        CurrentValue = 1000m,
                        Tranches =
                        [
                            new AssetTrancheDto { PurchaseDate = new DateOnly(2020, 1, 1), Quantity = 5m, AcquisitionPricePerUnit = 90m },
                            new AssetTrancheDto { PurchaseDate = new DateOnly(2020, 2, 1), Quantity = 5m, AcquisitionPricePerUnit = 110m }
                        ]
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(0m, state.Cash);
        Assert.Equal(4m, state.Portfolio.Assets[0].CurrentQuantity);
        var tranches = state.Portfolio.Assets[0].Tranches;
        Assert.Single(tranches);
        Assert.Equal(new DateOnly(2020, 2, 1), tranches[0].PurchaseDate);
        Assert.Equal(4m, tranches[0].Quantity);
        Assert.Equal(110m, tranches[0].AcquisitionPricePerUnit);
    }

    [Fact]
    public void ProcessMonth_CashNegative_FifoPartialConsumeSecondTranche()
    {
        var processor = new LiquidityProcessor();
        var project = Project();
        var state = new SimulationState
        {
            Cash = -250m,
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        AssetClassId = ClassA,
                        CurrentPrice = 100m,
                        CurrentQuantity = 5m,
                        CurrentValue = 500m,
                        Tranches =
                        [
                            new AssetTrancheDto { PurchaseDate = new DateOnly(2020, 1, 1), Quantity = 2m, AcquisitionPricePerUnit = 90m },
                            new AssetTrancheDto { PurchaseDate = new DateOnly(2020, 2, 1), Quantity = 3m, AcquisitionPricePerUnit = 110m }
                        ]
                    }
                ]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(-50m, state.Cash);
        Assert.Equal(3m, state.Portfolio.Assets[0].CurrentQuantity);
        var tranches = state.Portfolio.Assets[0].Tranches;
        Assert.Single(tranches);
        Assert.Equal(new DateOnly(2020, 2, 1), tranches[0].PurchaseDate);
        Assert.Equal(3m, tranches[0].Quantity);
    }

    [Fact]
    public void ProcessMonth_TotalAssets_UpdatedAfterInvest()
    {
        var processor = new LiquidityProcessor();
        var profileId = "p1";
        var project = Project(
            profiles: [new AllocationProfileDto { Id = profileId, Entries = [new AllocationProfileEntryDto { AssetClassId = ClassA, TargetWeight = 1.0m }] }],
            phases: [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = profileId }]);
        var state = new SimulationState
        {
            Cash = 1000m,
            TotalAssets = 1000m,
            Portfolio = new PortfolioDto
            {
                Assets = [new AssetDto { Id = "a1", AssetClassId = ClassA, CurrentPrice = 100m, CurrentQuantity = 0m, CurrentValue = 0m, IsActiveSavingsInstrument = true }]
            }
        };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(1000m, state.TotalAssets); // 0 cash + 1000 portfolio
    }
}
