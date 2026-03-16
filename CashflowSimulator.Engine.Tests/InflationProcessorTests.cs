using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

public sealed class InflationProcessorTests
{
    private const string InflationFactorId = "Inflation";

    private static SimulationProjectDto Project(
        List<CashflowStreamDto> streams,
        List<EconomicFactorDto>? economicFactors = null,
        SimulationParametersDto? parameters = null)
    {
        return new SimulationProjectDto
        {
            Parameters = parameters ?? new SimulationParametersDto
            {
                SimulationStart = new DateOnly(2020, 1, 1),
                SimulationEnd = new DateOnly(2022, 12, 1),
                DateOfBirth = new DateOnly(1985, 1, 1),
                InitialLiquidCash = 0m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams = streams,
            EconomicFactors = economicFactors ?? []
        };
    }

    [Fact]
    public void ProcessMonth_NoStreams_DoesNotThrow_IndexedAmountsEmpty()
    {
        var processor = new InflationProcessor();
        var project = Project([]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Empty(state.IndexedStreamAmounts);
    }

    [Fact]
    public void ProcessMonth_FirstMonth_InitializesIndexedStreamAmountsFromProject()
    {
        var processor = new InflationProcessor();
        var stream1 = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Gehalt",
            Amount = 3000m,
            EconomicFactorId = InflationFactorId
        };
        var stream2 = new CashflowStreamDto
        {
            Id = "s2",
            Name = "Miete",
            Amount = 900m,
            EconomicFactorId = null
        };
        var project = Project([stream1, stream2], [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.02 }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        Assert.Equal(2, state.IndexedStreamAmounts.Count);
        Assert.Equal(3000m, state.IndexedStreamAmounts["s1"]);
        Assert.Equal(900m, state.IndexedStreamAmounts["s2"]);
    }

    [Fact]
    public void ProcessMonth_January_SecondYear_AppliesInflationToStreamWithEconomicFactorId()
    {
        var processor = new InflationProcessor();
        var streamLinked = new CashflowStreamDto
        {
            Id = "linked",
            Name = "Gehalt",
            Amount = 1000m,
            EconomicFactorId = InflationFactorId
        };
        var streamUnlinked = new CashflowStreamDto
        {
            Id = "unlinked",
            Name = "Miete",
            Amount = 500m,
            EconomicFactorId = null
        };
        var project = Project(
            [streamLinked, streamUnlinked],
            [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.02 }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        for (var m = 2; m <= 12; m++)
            processor.ProcessMonth(project, state, new DateOnly(2020, m, 1));
        processor.ProcessMonth(project, state, new DateOnly(2021, 1, 1));

        var expectedLinked = 1000m * (decimal)Math.Exp(0.02);
        Assert.True(Math.Abs(state.IndexedStreamAmounts["linked"] - expectedLinked) < 0.01m);
        Assert.Equal(500m, state.IndexedStreamAmounts["unlinked"]);
    }

    [Fact]
    public void ProcessMonth_EveryMonthAfterSimStart_AppliesMonthlyInflation()
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Gehalt",
            Amount = 1000m,
            EconomicFactorId = InflationFactorId
        };
        var project = Project([stream], [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.10 }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        Assert.Equal(1000m, state.IndexedStreamAmounts["s1"]);
        processor.ProcessMonth(project, state, new DateOnly(2020, 2, 1));
        var expectedOneMonth = 1000m * (decimal)Math.Exp(0.10 / 12.0);
        Assert.True(Math.Abs(state.IndexedStreamAmounts["s1"] - expectedOneMonth) < 0.01m);
    }

    [Fact]
    public void ProcessMonth_TwelveMonths_MatchesAnnualContinuousInflation()
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Gehalt",
            Amount = 1000m,
            EconomicFactorId = InflationFactorId
        };
        var project = Project([stream], [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.02 }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        for (var m = 2; m <= 12; m++)
            processor.ProcessMonth(project, state, new DateOnly(2020, m, 1));
        processor.ProcessMonth(project, state, new DateOnly(2021, 1, 1));

        var expected = 1000m * (decimal)Math.Exp(0.02);
        Assert.True(Math.Abs(state.IndexedStreamAmounts["s1"] - expected) < 0.01m);
    }

    [Fact]
    public void ProcessMonth_StreamWithoutEconomicFactorId_UnchangedAfterTwoJanuaries()
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Miete",
            Amount = 800m,
            EconomicFactorId = null
        };
        var project = Project([stream], [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.05 }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        processor.ProcessMonth(project, state, new DateOnly(2021, 1, 1));
        processor.ProcessMonth(project, state, new DateOnly(2022, 1, 1));

        Assert.Equal(800m, state.IndexedStreamAmounts["s1"]);
    }

    [Fact]
    public void ProcessMonth_TwentyFourMonths_CompoundsInflation()
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Gehalt",
            Amount = 1000m,
            EconomicFactorId = InflationFactorId
        };
        var project = Project([stream], [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.10 }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        Assert.Equal(1000m, state.IndexedStreamAmounts["s1"]);

        for (var i = 0; i < 24; i++)
            processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1).AddMonths(i + 1));

        var expected = 1000m * (decimal)Math.Exp(0.10 * 2);
        Assert.True(Math.Abs(state.IndexedStreamAmounts["s1"] - expected) < 0.01m);
    }

    [Fact]
    public void ProcessMonth_UnknownEconomicFactorId_LeavesAmountUnchanged()
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Gehalt",
            Amount = 2000m,
            EconomicFactorId = "NonExistentFactor"
        };
        var project = Project([stream], [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.02 }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        processor.ProcessMonth(project, state, new DateOnly(2021, 1, 1));

        Assert.Equal(2000m, state.IndexedStreamAmounts["s1"]);
    }

    [Fact]
    public void ProcessMonth_EmptyEconomicFactors_DoesNotThrow()
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Gehalt",
            Amount = 1000m,
            EconomicFactorId = InflationFactorId
        };
        var project = Project([stream], []);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        processor.ProcessMonth(project, state, new DateOnly(2021, 1, 1));

        Assert.Equal(1000m, state.IndexedStreamAmounts["s1"]);
    }

    [Fact]
    public void ProcessMonth_JanuaryAtSimStart_DoesNotApplyInflation()
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Gehalt",
            Amount = 1000m,
            EconomicFactorId = InflationFactorId
        };
        var project = Project(
            [stream],
            [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = 0.02 }],
            new SimulationParametersDto
            {
                SimulationStart = new DateOnly(2020, 1, 1),
                SimulationEnd = new DateOnly(2021, 6, 1),
                DateOfBirth = new DateOnly(1985, 1, 1),
                InitialLiquidCash = 0m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            });
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        Assert.Equal(1000m, state.IndexedStreamAmounts["s1"]);
    }

    /// <summary>
    /// Spezifikation für Agents: Inflationsformel amount_after = amount_initial * e^(annualRate * months/12).
    /// Erster Monat (SimStart) initialisiert nur; danach wird jeden Monat mit e^(rate/12) multipliziert.
    /// </summary>
    [Theory]
    [InlineData(1000, 0.02, 12, 1020.201)]
    [InlineData(500, 0.05, 6, 512.66)]
    [InlineData(2000, 0, 12, 2000)]
    [InlineData(100, 0.10, 1, 100.84)]
    public void ProcessMonth_InflationFormula_AmountAfterMonths_MatchesSpec(
        decimal initialAmount,
        double annualRate,
        int monthsAfterInit,
        decimal expectedApprox)
    {
        var processor = new InflationProcessor();
        var stream = new CashflowStreamDto
        {
            Id = "s1",
            Name = "Stream",
            Amount = initialAmount,
            EconomicFactorId = InflationFactorId
        };
        var project = Project(
            [stream],
            [new EconomicFactorDto { Id = InflationFactorId, ExpectedReturn = annualRate }]);
        var state = new SimulationState();

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));
        for (var i = 1; i <= monthsAfterInit; i++)
            processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1).AddMonths(i));

        var actual = state.IndexedStreamAmounts["s1"];
        var tolerance = 0.02m;
        Assert.True(Math.Abs(actual - expectedApprox) <= tolerance,
            $"Expected ~{expectedApprox}, got {actual}. Formula: amount * e^(rate*{monthsAfterInit}/12) = {initialAmount * (decimal)Math.Exp(annualRate * monthsAfterInit / 12.0)}");
    }

    [Fact]
    public async Task SimulationRunner_WithInflationProcessor_SecondYearSnapshotAmountHigher()
    {
        var repo = new InMemorySimulationResultRepository();
        ISimulationRunner runner = new SimulationRunner(
        [
            new InflationProcessor(),
            new CashflowProcessor(),
            new LiquidityProcessor(),
            new GrowthProcessor()
        ],
            repo);
        var stream = new CashflowStreamDto
        {
            Id = "salary",
            Name = "Gehalt",
            Type = CashflowType.Income,
            Amount = 3000m,
            Interval = CashflowInterval.Monthly,
            StartDate = new DateOnly(2020, 1, 1),
            EndDate = null,
            EconomicFactorId = InflationFactorId
        };
        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto
            {
                SimulationStart = new DateOnly(2020, 1, 1),
                SimulationEnd = new DateOnly(2021, 12, 1),
                DateOfBirth = new DateOnly(1985, 1, 1),
                InitialLiquidCash = 0m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams = [stream],
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

        var monthly = await repo.GetMonthlyResultsAsync(result.RunId!.Value, TestContext.Current.CancellationToken);
        var jan2020 = monthly[0];
        var jan2021 = monthly[12];
        var salarySnapshot2020 = jan2020.CashflowSnapshots.FirstOrDefault(s => s.Name == "Gehalt");
        var salarySnapshot2021 = jan2021.CashflowSnapshots.FirstOrDefault(s => s.Name == "Gehalt");

        Assert.NotNull(salarySnapshot2020);
        Assert.NotNull(salarySnapshot2021);
        Assert.Equal(3000m, salarySnapshot2020.Amount);
        var expectedSalary2021 = 3000m * (decimal)Math.Exp(0.02);
        Assert.True(Math.Abs(salarySnapshot2021.Amount - expectedSalary2021) < 0.01m);
    }
}
