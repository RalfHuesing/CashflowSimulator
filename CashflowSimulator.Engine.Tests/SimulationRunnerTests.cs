using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

public sealed class SimulationRunnerTests
{
    private static ISimulationRunner CreateRunner() =>
        new SimulationRunner([new CashflowProcessor(), new GrowthProcessor()]);

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
    public void RunSimulation_Throws_WhenProjectIsNull()
    {
        var runner = CreateRunner();
        Assert.Throws<ArgumentNullException>(() => runner.RunSimulation(null!));
    }

    [Fact]
    public void RunSimulation_EmptyDateRange_ReturnsEmptyMonthlyResults()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 12, 1),
            end: new DateOnly(2020, 1, 1));
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.NotNull(result.MonthlyResults);
        Assert.Empty(result.MonthlyResults);
    }

    [Fact]
    public void RunSimulation_OneMonth_NoStreams_ReturnsOneResultWithInitialCash()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 1, 1),
            initialCash: 15_000m);
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Single(result.MonthlyResults);
        var month = result.MonthlyResults[0];
        Assert.Equal(0, month.MonthIndex);
        Assert.Equal(15_000m, month.CashBalance);
        Assert.Equal(15_000m, month.TotalAssets);
        Assert.Empty(month.CashflowSnapshots);
    }

    [Fact]
    public void RunSimulation_TwelveMonths_NoStreams_ReturnsTwelveResults_SameCash()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 12, 1),
            initialCash: 20_000m);
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(12, result.MonthlyResults.Count);
        foreach (var m in result.MonthlyResults)
            Assert.Equal(20_000m, m.CashBalance);
    }

    [Fact]
    public void RunSimulation_MonthlyIncome_IncreasesCash()
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
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(3, result.MonthlyResults.Count);
        Assert.Equal(1500m, result.MonthlyResults[0].CashBalance);  // 1000 + 500
        Assert.Equal(2000m, result.MonthlyResults[1].CashBalance);
        Assert.Equal(2500m, result.MonthlyResults[2].CashBalance);
        foreach (var m in result.MonthlyResults)
        {
            Assert.Single(m.CashflowSnapshots);
            Assert.Equal("Gehalt", m.CashflowSnapshots[0].Name);
            Assert.Equal(CashflowType.Income, m.CashflowSnapshots[0].CashflowType);
            Assert.Equal(500m, m.CashflowSnapshots[0].Amount);
        }
    }

    [Fact]
    public void RunSimulation_MonthlyExpense_DecreasesCash()
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
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(2, result.MonthlyResults.Count);
        Assert.Equal(3800m, result.MonthlyResults[0].CashBalance);
        Assert.Equal(2600m, result.MonthlyResults[1].CashBalance);
    }

    [Fact]
    public void RunSimulation_StreamActiveOnlyAfterStartDate_AppliedFromStart()
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
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(0m, result.MonthlyResults[0].CashBalance);
        Assert.Empty(result.MonthlyResults[0].CashflowSnapshots);
        Assert.Equal(100m, result.MonthlyResults[1].CashBalance);
        Assert.Equal(200m, result.MonthlyResults[2].CashBalance);
    }

    [Fact]
    public void RunSimulation_StreamEndDate_StopsAfterEnd()
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
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(50m, result.MonthlyResults[0].CashBalance);
        Assert.Equal(100m, result.MonthlyResults[1].CashBalance);
        Assert.Equal(100m, result.MonthlyResults[2].CashBalance);
        Assert.Equal(100m, result.MonthlyResults[3].CashBalance);
    }

    [Fact]
    public void RunSimulation_YearlyStream_AppliedOnlyInJanuary()
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
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(12, result.MonthlyResults.Count);
        Assert.Equal(1200m, result.MonthlyResults[0].CashBalance);
        for (var i = 1; i < 12; i++)
            Assert.Equal(1200m, result.MonthlyResults[i].CashBalance);
        Assert.Single(result.MonthlyResults[0].CashflowSnapshots);
        for (var i = 1; i < 12; i++)
            Assert.Empty(result.MonthlyResults[i].CashflowSnapshots);
    }

    [Fact]
    public void RunSimulation_Age_AdvancesWithMonths()
    {
        var project = MinimalProject(
            start: new DateOnly(2020, 1, 1),
            end: new DateOnly(2020, 12, 1));
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(12, result.MonthlyResults.Count);
        var age0 = result.MonthlyResults[0].Age;
        var age11 = result.MonthlyResults[11].Age;
        Assert.True(age11 > age0);
        Assert.True(age0 >= 34 && age0 < 35);
        Assert.True(age11 >= 34 && age11 < 36);
    }

    [Fact]
    public void RunSimulation_TotalAssets_EqualsCashInSlice1()
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
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        foreach (var m in result.MonthlyResults)
            Assert.Equal(m.CashBalance, m.TotalAssets);
    }

    [Fact]
    public void RunSimulation_WithPortfolioAndEconomicFactor_TotalAssetsIncludesGrownPortfolio()
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
        var runner = CreateRunner();

        var result = runner.RunSimulation(project);

        Assert.Equal(12, result.MonthlyResults.Count);
        var firstMonth = result.MonthlyResults[0];
        Assert.Equal(1000m, firstMonth.CashBalance);
        Assert.True(firstMonth.TotalAssets > 1000m, "TotalAssets soll Cash + gewachsenes Depot sein (1010 bei 1% Monatswachstum).");

        var lastMonth = result.MonthlyResults[11];
        Assert.True(lastMonth.TotalAssets > lastMonth.CashBalance, "Nach 12 Monaten Wachstum: TotalAssets > Cash.");
        Assert.True(lastMonth.TotalAssets >= 2060m, "~6% p.a. über 12 Monate: Depotwert wächst von 1000 auf ca. 1062; TotalAssets mind. 2060.");
    }
}
