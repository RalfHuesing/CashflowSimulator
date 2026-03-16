using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

/// <summary>
/// E2E-Integrationstests: Lang laufende Szenarien (Grand Tour) zur logischen Konsistenz von CashBalance und TotalAssets
/// über Jahrzehnte (Meilensteine und Endwert, keine Monat-für-Monat-Assertions).
/// </summary>
public sealed class SimulationIntegrationTests
{
    private const string InflationFactorId = "Inflation";
    private const string EquityFactorId = "MSCI_World";

    private static (ISimulationRunner Runner, InMemorySimulationResultRepository Repository) CreateRunnerWithInflation()
    {
        var repo = new InMemorySimulationResultRepository();
        var runner = new SimulationRunner(
        [
            new InflationProcessor(),
            new CashflowProcessor(),
            new LiquidityProcessor(),
            new GrowthProcessor()
        ],
            repo);
        return (runner, repo);
    }

    /// <summary>
    /// Nutzer 30 Jahre, 50k Startkapital, 1k monatliche Sparrate, 2 % Inflation, Laufzeit bis 67. Lebensjahr (37 Jahre).
    /// </summary>
    [Fact]
    public async Task GrandTour_30YearOld_50kStart_1kMonthly_2PercentInflation_ToAge67_ConsistentCashAndTotalAssets()
    {
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var simulationStart = new DateOnly(2020, 1, 1);
        var simulationEnd = new DateOnly(2057, 1, 1);
        var (runner, repo) = CreateRunnerWithInflation();

        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto
            {
                SimulationStart = simulationStart,
                SimulationEnd = simulationEnd,
                DateOfBirth = dateOfBirth,
                InitialLiquidCash = 50_000m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams =
            [
                new CashflowStreamDto
                {
                    Id = "salary",
                    Name = "Gehalt",
                    Type = CashflowType.Income,
                    Amount = 1000m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = simulationStart,
                    EndDate = null,
                    EconomicFactorId = InflationFactorId
                }
            ],
            EconomicFactors =
            [
                new EconomicFactorDto
                {
                    Id = InflationFactorId,
                    Name = "Inflation",
                    ExpectedReturn = 0.02
                }
            ]
        };

        var result = await runner.RunSimulationAsync(project, TestContext.Current.CancellationToken);
        Assert.NotNull(result.RunId);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value, TestContext.Current.CancellationToken);
        var monthCount = (simulationEnd.Year - simulationStart.Year) * 12 + (simulationEnd.Month - simulationStart.Month) + 1;
        Assert.Equal(monthCount, monthly.Count);

        var firstMonth = monthly[0];
        Assert.Equal(51_000m, firstMonth.CashBalance);
        Assert.Equal(firstMonth.CashBalance, firstMonth.TotalAssets);
        Assert.True(firstMonth.CashBalance >= 0);

        var after12Months = monthly[11];
        Assert.True(after12Months.CashBalance > 50_000m, "Nach 12 Monaten: Sparrate + Startkapital erhöhen Cash.");
        Assert.Equal(after12Months.CashBalance, after12Months.TotalAssets);
        Assert.True(after12Months.CashBalance >= 0);

        var lastMonth = monthly[^1];
        Assert.True(lastMonth.CashBalance >= 0);
        Assert.Equal(lastMonth.CashBalance, lastMonth.TotalAssets);
        Assert.True(lastMonth.TotalAssets >= 50_000m, "Endvermögen mindestens Startkapital + kumulierte Sparrate über Laufzeit.");
    }

    /// <summary>
    /// Wie GrandTour, aber mit Portfolio (ETF): TotalAssets > Cash nach Wachstum.
    /// </summary>
    [Fact]
    public async Task GrandTour_WithPortfolio_TotalAssetsExceedsCashAfterGrowth()
    {
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var simulationStart = new DateOnly(2020, 1, 1);
        var simulationEnd = new DateOnly(2022, 12, 1);
        var (runner, repo) = CreateRunnerWithInflation();

        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto
            {
                SimulationStart = simulationStart,
                SimulationEnd = simulationEnd,
                DateOfBirth = dateOfBirth,
                InitialLiquidCash = 50_000m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams =
            [
                new CashflowStreamDto
                {
                    Id = "salary",
                    Name = "Gehalt",
                    Type = CashflowType.Income,
                    Amount = 1000m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = simulationStart,
                    EndDate = null,
                    EconomicFactorId = InflationFactorId
                }
            ],
            EconomicFactors =
            [
                new EconomicFactorDto { Id = InflationFactorId, Name = "Inflation", ExpectedReturn = 0.02 },
                new EconomicFactorDto
                {
                    Id = EquityFactorId,
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
                        Id = "etf1",
                        Name = "ETF",
                        EconomicFactorId = EquityFactorId,
                        CurrentPrice = 100m,
                        CurrentQuantity = 100m,
                        CurrentValue = 10_000m
                    }
                ]
            }
        };

        var result = await runner.RunSimulationAsync(project, TestContext.Current.CancellationToken);
        Assert.NotNull(result.RunId);

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value, TestContext.Current.CancellationToken);
        var firstMonth = monthly[0];
        Assert.True(firstMonth.TotalAssets >= 60_000m);
        Assert.True(firstMonth.TotalAssets > firstMonth.CashBalance, "TotalAssets = Cash + Depot.");

        var lastMonth = monthly[^1];
        Assert.True(lastMonth.TotalAssets > lastMonth.CashBalance);
        Assert.True(lastMonth.TotalAssets >= 60_000m, "Portfolio wächst (6 % p.a.), Endwert höher als Start.");
    }

    /// <summary>
    /// Längere Laufzeit (z. B. 10 Jahre): Meilenstein nach 120 Monaten, Endwert plausibel.
    /// </summary>
    [Fact]
    public async Task GrandTour_TenYears_MilestoneAt120Months_EndValuePlausible()
    {
        var simulationStart = new DateOnly(2020, 1, 1);
        var simulationEnd = new DateOnly(2030, 1, 1);
        var (runner, repo) = CreateRunnerWithInflation();

        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto
            {
                SimulationStart = simulationStart,
                SimulationEnd = simulationEnd,
                DateOfBirth = new DateOnly(1990, 1, 1),
                InitialLiquidCash = 20_000m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams =
            [
                new CashflowStreamDto
                {
                    Id = "s1",
                    Name = "Einnahme",
                    Type = CashflowType.Income,
                    Amount = 500m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = simulationStart,
                    EndDate = null,
                    EconomicFactorId = InflationFactorId
                }
            ],
            EconomicFactors =
            [
                new EconomicFactorDto { Id = InflationFactorId, Name = "Inflation", ExpectedReturn = 0.02 }
            ]
        };

        var result = await runner.RunSimulationAsync(project, TestContext.Current.CancellationToken);
        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value, TestContext.Current.CancellationToken);

        Assert.Equal(121, monthly.Count);

        var at120Months = monthly[119];
        Assert.True(at120Months.CashBalance >= 20_000m + 119 * 500m - 1000m, "Grob: Start + 119 Monate Einnahme mindestens.");
        Assert.Equal(at120Months.CashBalance, at120Months.TotalAssets);

        var last = monthly[^1];
        Assert.True(last.CashBalance >= 0);
        Assert.True(last.TotalAssets >= 20_000m);
    }
}
