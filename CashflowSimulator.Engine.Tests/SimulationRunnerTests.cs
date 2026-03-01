using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

public sealed class SimulationRunnerTests
{
    private static (ISimulationRunner Runner, InMemorySimulationResultRepository Repository) CreateRunnerWithRepository()
    {
        var repo = new InMemorySimulationResultRepository();
        var runner = new SimulationRunner(
            [new CashflowProcessor(), new LiquidityProcessor(), new GrowthProcessor()],
            repo);
        return (runner, repo);
    }

    private static SimulationProjectDto MinimalProject(
        DateOnly? start = null,
        DateOnly? end = null,
        decimal initialCash = 10_000m,
        List<CashflowStreamDto>? streams = null)
    {
        var s = start ?? new DateOnly(2020, 1, 1);
        var e = end ?? new DateOnly(2020, 12, 1);
        return new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto
            {
                SimulationStart = s,
                SimulationEnd = e,
                DateOfBirth = new DateOnly(1985, 6, 15),
                InitialLiquidCash = initialCash,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams = streams ?? []
        };
    }

    [Fact]
    public async Task RunSimulationAsync_Throws_WhenProjectIsNull()
    {
        var (runner, _) = CreateRunnerWithRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await runner.RunSimulationAsync(null!));
    }

    [Fact]
    public async Task RunSimulationAsync_EmptyDateRange_ReturnsEmptyMonthlyResults()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 12, 1),
            end: new DateOnly(2020, 1, 1));
        var (runner, _) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        Assert.Null(result.RunId);
        Assert.NotNull(result.MonthlyResults);
        Assert.Empty(result.MonthlyResults);
    }

    [Fact]
    public async Task RunSimulationAsync_OneMonth_NoStreams_ReturnsOneResultWithInitialCash()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 1, 1),
            initialCash: 15_000m);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        Assert.NotNull(result.RunId);
        var monthly = await repo.GetMonthlyResultsAsync(result.RunId.Value);
        Assert.Single(monthly);
        var month = monthly[0];
        Assert.Equal(0, month.MonthIndex);
        Assert.Equal(15_000m, month.CashBalance);
        Assert.Equal(15_000m, month.TotalAssets);
        Assert.Empty(month.CashflowSnapshots);
    }

    [Fact]
    public async Task RunSimulationAsync_TwelveMonths_NoStreams_ReturnsTwelveResults_SameCash()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 12, 1),
            initialCash: 20_000m);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(12, monthly.Count);
        foreach (var m in monthly)
            Assert.Equal(20_000m, m.CashBalance);
    }

    [Fact]
    public async Task RunSimulationAsync_MonthlyIncome_IncreasesCash()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 3, 1),
            initialCash: 1000m,
            streams:
            [
                new CashflowStreamDto
                {
                    Name = "Gehalt",
                    Type = CashflowType.Income,
                    Amount = 500m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = new DateOnly(2020, 1, 1),
                    EndDate = null
                }
            ]);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(3, monthly.Count);
        Assert.Equal(1500m, monthly[0].CashBalance);  // 1000 + 500
        Assert.Equal(2000m, monthly[1].CashBalance);
        Assert.Equal(2500m, monthly[2].CashBalance);
        foreach (var m in monthly)
        {
            Assert.Single(m.CashflowSnapshots);
            Assert.Equal("Gehalt", m.CashflowSnapshots[0].Name);
            Assert.Equal(CashflowType.Income, m.CashflowSnapshots[0].CashflowType);
            Assert.Equal(500m, m.CashflowSnapshots[0].Amount);
        }
    }

    [Fact]
    public async Task RunSimulationAsync_MonthlyExpense_DecreasesCash()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 2, 1),
            initialCash: 5000m,
            streams:
            [
                new CashflowStreamDto
                {
                    Name = "Miete",
                    Type = CashflowType.Expense,
                    Amount = 1200m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = new DateOnly(2020, 1, 1),
                    EndDate = null
                }
            ]);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(2, monthly.Count);
        Assert.Equal(3800m, monthly[0].CashBalance);
        Assert.Equal(2600m, monthly[1].CashBalance);
    }

    [Fact]
    public async Task RunSimulationAsync_StreamActiveOnlyAfterStartDate_AppliedFromStart()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 3, 1),
            initialCash: 0m,
            streams:
            [
                new CashflowStreamDto
                {
                    Name = "Bonus",
                    Type = CashflowType.Income,
                    Amount = 100m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = new DateOnly(2020, 2, 1),
                    EndDate = null
                }
            ]);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(0m, monthly[0].CashBalance);
        Assert.Empty(monthly[0].CashflowSnapshots);
        Assert.Equal(100m, monthly[1].CashBalance);
        Assert.Equal(200m, monthly[2].CashBalance);
    }

    [Fact]
    public async Task RunSimulationAsync_StreamEndDate_StopsAfterEnd()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 4, 1),
            initialCash: 0m,
            streams:
            [
                new CashflowStreamDto
                {
                    Name = "Temp",
                    Type = CashflowType.Income,
                    Amount = 50m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = new DateOnly(2020, 1, 1),
                    EndDate = new DateOnly(2020, 2, 1)
                }
            ]);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(50m, monthly[0].CashBalance);
        Assert.Equal(100m, monthly[1].CashBalance);
        Assert.Equal(100m, monthly[2].CashBalance);
        Assert.Equal(100m, monthly[3].CashBalance);
    }

    [Fact]
    public async Task RunSimulationAsync_YearlyStream_AppliedOnlyInJanuary()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 12, 1),
            initialCash: 0m,
            streams:
            [
                new CashflowStreamDto
                {
                    Name = "Jahresbonus",
                    Type = CashflowType.Income,
                    Amount = 1200m,
                    Interval = CashflowInterval.Yearly,
                    StartDate = new DateOnly(2020, 1, 1),
                    EndDate = null
                }
            ]);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(12, monthly.Count);
        Assert.Equal(1200m, monthly[0].CashBalance);
        for (var i = 1; i < 12; i++)
            Assert.Equal(1200m, monthly[i].CashBalance);
        Assert.Single(monthly[0].CashflowSnapshots);
        for (var i = 1; i < 12; i++)
            Assert.Empty(monthly[i].CashflowSnapshots);
    }

    [Fact]
    public async Task RunSimulationAsync_Age_AdvancesWithMonths()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 12, 1));
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(12, monthly.Count);
        var age0 = monthly[0].Age;
        var age11 = monthly[11].Age;
        Assert.True(age11 > age0);
        Assert.True(age0 >= 34 && age0 < 35);
        Assert.True(age11 >= 34 && age11 < 36);
    }

    [Fact]
    public async Task RunSimulationAsync_TotalAssets_EqualsCashInSlice1()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 2, 1),
            initialCash: 100m,
            streams:
            [
                new CashflowStreamDto
                {
                    Name = "E",
                    Type = CashflowType.Income,
                    Amount = 10m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = new DateOnly(2020, 1, 1),
                    EndDate = null
                }
            ]);
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        foreach (var m in monthly)
            Assert.Equal(m.CashBalance, m.TotalAssets);
    }

    [Fact]
    public async Task RunSimulationAsync_WithPortfolioAndEconomicFactor_TotalAssetsIncludesGrownPortfolio()
    {
        const string factorId = "MSCI_World";
        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto
            {
                SimulationStart = new DateOnly(2020, 1, 1),
                SimulationEnd = new DateOnly(2020, 12, 1),
                DateOfBirth = new DateOnly(1985, 6, 15),
                InitialLiquidCash = 1000m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams = [],
            EconomicFactors =
            [
                new EconomicFactorDto
                {
                    Id = factorId,
                    Name = "MSCI World",
                    ModelType = StochasticModelType.GeometricBrownianMotion,
                    ExpectedReturn = 0.06,
                    Volatility = 0.18,
                    InitialValue = 100.0
                }
            ],
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "asset1",
                        Name = "ETF",
                        EconomicFactorId = factorId,
                        CurrentPrice = 100m,
                        CurrentQuantity = 10m,
                        CurrentValue = 1000m
                    }
                ]
            }
        };
        var (runner, repo) = CreateRunnerWithRepository();

        var result = await runner.RunSimulationAsync(project);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(12, monthly.Count);
        var firstMonth = monthly[0];
        Assert.Equal(1000m, firstMonth.CashBalance);
        Assert.True(firstMonth.TotalAssets > 1000m, "TotalAssets soll Cash + gewachsenes Depot sein (1010 bei 1% Monatswachstum).");

        var lastMonth = monthly[11];
        Assert.True(lastMonth.TotalAssets > lastMonth.CashBalance, "Nach 12 Monaten Wachstum: TotalAssets > Cash.");
        Assert.True(lastMonth.TotalAssets >= 2060m, "~6% p.a. über 12 Monate: Depotwert wächst von 1000 auf ca. 1062; TotalAssets mind. 2060.");
    }

    [Fact]
    public async Task RunSimulationAsync_ClonesPortfolio_DoesNotMutateProjectTranchesOrTransactions()
    {
        var tranches = new List<AssetTrancheDto> { new() { PurchaseDate = new DateOnly(2020, 1, 1), Quantity = 10m, AcquisitionPricePerUnit = 100m } };
        var transactions = new List<TransactionDto> { new() { Date = new DateOnly(2020, 1, 1), Type = TransactionType.Buy, Quantity = 10m, PricePerUnit = 100m, TotalAmount = 1000m } };
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 2, 1),
            initialCash: 0m);
        project = project with
        {
            EconomicFactors = [new EconomicFactorDto { Id = "f1", Name = "F", ModelType = StochasticModelType.GeometricBrownianMotion, ExpectedReturn = 0, Volatility = 0.1, InitialValue = 100 }],
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        Name = "ETF",
                        EconomicFactorId = "f1",
                        CurrentPrice = 100m,
                        CurrentQuantity = 10m,
                        CurrentValue = 1000m,
                        Tranches = tranches,
                        Transactions = transactions
                    }
                ]
            }
        };

        var (runner, _) = CreateRunnerWithRepository();
        await runner.RunSimulationAsync(project);

        Assert.Same(tranches, project.Portfolio!.Assets[0].Tranches);
        Assert.Same(transactions, project.Portfolio.Assets[0].Transactions);
    }

    [Fact]
    public async Task RunSimulationAsync_EmptyTranchesAndBuyTransactions_CompletesWithoutError()
    {
        var transactions = new List<TransactionDto>
        {
            new() { Date = new DateOnly(2020, 1, 15), Type = TransactionType.Buy, Quantity = 5m, PricePerUnit = 90m, TotalAmount = 450m },
            new() { Date = new DateOnly(2020, 2, 10), Type = TransactionType.Buy, Quantity = 3m, PricePerUnit = 100m, TotalAmount = 300m }
        };
        var project = MinimalProject(start: new DateOnly(2020, 1, 1), end: new DateOnly(2020, 3, 1), initialCash: 0m);
        project = project with
        {
            EconomicFactors = [new EconomicFactorDto { Id = "f1", Name = "F", ModelType = StochasticModelType.GeometricBrownianMotion, ExpectedReturn = 0, Volatility = 0.1, InitialValue = 100 }],
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        Name = "ETF",
                        EconomicFactorId = "f1",
                        CurrentPrice = 100m,
                        CurrentQuantity = 8m,
                        CurrentValue = 800m,
                        Tranches = [],
                        Transactions = transactions
                    }
                ]
            }
        };
        var (runner, repo) = CreateRunnerWithRepository();
        var result = await runner.RunSimulationAsync(project);

        Assert.NotNull(result);
        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value);
        Assert.Equal(3, monthly.Count);
        Assert.True(monthly[0].TotalAssets >= 800m);
    }
}
