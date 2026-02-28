using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

public sealed class CashflowProcessorTests
{
    private static SimulationProjectDto Project(
        List<CashflowStreamDto> streams,
        SimulationParametersDto? parameters = null)
    {
        return new SimulationProjectDto
        {
            Parameters = parameters ?? new SimulationParametersDto
            {
                SimulationStart = new DateOnly(2020, 1, 1),
                SimulationEnd = new DateOnly(2020, 12, 1),
                DateOfBirth = new DateOnly(1985, 1, 1),
                InitialLiquidCash = 0m,
                InitialLossCarryforwardGeneral = 0,
                InitialLossCarryforwardStocks = 0
            },
            Streams = streams
        };
    }

    [Fact]
    public void ProcessMonth_NoStreams_LeavesCashAndSnapshotsUnchanged()
    {
        var processor = new CashflowProcessor();
        var project = Project([]);
        var state = new SimulationState { Cash = 1000m };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(1000m, state.Cash);
        Assert.Empty(state.CurrentMonthSnapshots);
    }

    [Fact]
    public void ProcessMonth_MonthlyIncome_IncreasesCashAndAddsSnapshot()
    {
        var processor = new CashflowProcessor();
        var project = Project(
        [
            new CashflowStreamDto
            {
                Name = "Gehalt",
                Type = CashflowType.Income,
                Amount = 3000m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = null
            }
        ]);
        var state = new SimulationState { Cash = 500m };

        processor.ProcessMonth(project, state, new DateOnly(2020, 3, 1));

        Assert.Equal(3500m, state.Cash);
        Assert.Single(state.CurrentMonthSnapshots);
        Assert.Equal("Gehalt", state.CurrentMonthSnapshots[0].Name);
        Assert.Equal(CashflowType.Income, state.CurrentMonthSnapshots[0].CashflowType);
        Assert.Equal(3000m, state.CurrentMonthSnapshots[0].Amount);
    }

    [Fact]
    public void ProcessMonth_MonthlyExpense_DecreasesCashAndAddsSnapshot()
    {
        var processor = new CashflowProcessor();
        var project = Project(
        [
            new CashflowStreamDto
            {
                Name = "Miete",
                Type = CashflowType.Expense,
                Amount = 900m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = null
            }
        ]);
        var state = new SimulationState { Cash = 5000m };

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        Assert.Equal(4100m, state.Cash);
        Assert.Single(state.CurrentMonthSnapshots);
        Assert.Equal(CashflowType.Expense, state.CurrentMonthSnapshots[0].CashflowType);
        Assert.Equal(900m, state.CurrentMonthSnapshots[0].Amount);
    }

    [Fact]
    public void ProcessMonth_StreamBeforeStartDate_NotApplied()
    {
        var processor = new CashflowProcessor();
        var project = Project(
        [
            new CashflowStreamDto
            {
                Name = "Bonus",
                Type = CashflowType.Income,
                Amount = 100m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 5, 1),
                EndDate = null
            }
        ]);
        var state = new SimulationState { Cash = 0m };

        processor.ProcessMonth(project, state, new DateOnly(2020, 2, 1));

        Assert.Equal(0m, state.Cash);
        Assert.Empty(state.CurrentMonthSnapshots);
    }

    [Fact]
    public void ProcessMonth_StreamAfterEndDate_NotApplied()
    {
        var processor = new CashflowProcessor();
        var project = Project(
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
        var state = new SimulationState { Cash = 100m };

        processor.ProcessMonth(project, state, new DateOnly(2020, 4, 1));

        Assert.Equal(100m, state.Cash);
        Assert.Empty(state.CurrentMonthSnapshots);
    }

    [Fact]
    public void ProcessMonth_YearlyStream_AppliedOnlyInJanuary()
    {
        var processor = new CashflowProcessor();
        var project = Project(
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
        var stateJan = new SimulationState { Cash = 0m };
        var stateFeb = new SimulationState { Cash = 0m };

        processor.ProcessMonth(project, stateJan, new DateOnly(2020, 1, 1));
        processor.ProcessMonth(project, stateFeb, new DateOnly(2020, 2, 1));

        Assert.Equal(1200m, stateJan.Cash);
        Assert.Single(stateJan.CurrentMonthSnapshots);
        Assert.Equal(0m, stateFeb.Cash);
        Assert.Empty(stateFeb.CurrentMonthSnapshots);
    }

    [Fact]
    public void ProcessMonth_MultipleStreams_AllAppliedInOrder()
    {
        var processor = new CashflowProcessor();
        var project = Project(
        [
            new CashflowStreamDto
            {
                Name = "Gehalt",
                Type = CashflowType.Income,
                Amount = 2500m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = null
            },
            new CashflowStreamDto
            {
                Name = "Miete",
                Type = CashflowType.Expense,
                Amount = 800m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = null
            }
        ]);
        var state = new SimulationState { Cash = 0m };

        processor.ProcessMonth(project, state, new DateOnly(2020, 1, 1));

        Assert.Equal(1700m, state.Cash);
        Assert.Equal(2, state.CurrentMonthSnapshots.Count);
    }

    [Fact]
    public void ProcessMonth_PensionStreamWithStartAge67_NotAppliedBefore67()
    {
        var processor = new CashflowProcessor();
        var parameters = new SimulationParametersDto
        {
            SimulationStart = new DateOnly(2020, 1, 1),
            SimulationEnd = new DateOnly(2030, 12, 1),
            DateOfBirth = new DateOnly(1954, 1, 1),
            InitialLiquidCash = 0m,
            InitialLossCarryforwardGeneral = 0,
            InitialLossCarryforwardStocks = 0
        };
        var project = Project(
        [
            new CashflowStreamDto
            {
                Name = "Rente",
                Type = CashflowType.Income,
                Amount = 2000m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = null,
                StartAge = 67
            }
        ], parameters);
        var state = new SimulationState { Cash = 100m };

        processor.ProcessMonth(project, state, new DateOnly(2020, 6, 1));

        Assert.Equal(100m, state.Cash);
        Assert.Empty(state.CurrentMonthSnapshots);
    }

    [Fact]
    public void ProcessMonth_PensionStreamWithStartAge67_AppliedFrom67()
    {
        var processor = new CashflowProcessor();
        var parameters = new SimulationParametersDto
        {
            SimulationStart = new DateOnly(2020, 1, 1),
            SimulationEnd = new DateOnly(2030, 12, 1),
            DateOfBirth = new DateOnly(1954, 1, 1),
            InitialLiquidCash = 0m,
            InitialLossCarryforwardGeneral = 0,
            InitialLossCarryforwardStocks = 0
        };
        var project = Project(
        [
            new CashflowStreamDto
            {
                Name = "Rente",
                Type = CashflowType.Income,
                Amount = 2000m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = null,
                StartAge = 67
            }
        ], parameters);
        var state = new SimulationState { Cash = 100m };

        processor.ProcessMonth(project, state, new DateOnly(2021, 1, 1));

        Assert.Equal(2100m, state.Cash);
        Assert.Single(state.CurrentMonthSnapshots);
        Assert.Equal("Rente", state.CurrentMonthSnapshots[0].Name);
        Assert.Equal(2000m, state.CurrentMonthSnapshots[0].Amount);
    }

    [Fact]
    public void ProcessMonth_StreamWithEndAge70_NotAppliedAfter70()
    {
        var processor = new CashflowProcessor();
        var parameters = new SimulationParametersDto
        {
            SimulationStart = new DateOnly(2020, 1, 1),
            SimulationEnd = new DateOnly(2030, 12, 1),
            DateOfBirth = new DateOnly(1950, 1, 1),
            InitialLiquidCash = 0m,
            InitialLossCarryforwardGeneral = 0,
            InitialLossCarryforwardStocks = 0
        };
        var project = Project(
        [
            new CashflowStreamDto
            {
                Name = "Temp",
                Type = CashflowType.Income,
                Amount = 500m,
                Interval = CashflowInterval.Monthly,
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = null,
                StartAge = 65,
                EndAge = 70
            }
        ], parameters);
        var state = new SimulationState { Cash = 0m };

        processor.ProcessMonth(project, state, new DateOnly(2016, 1, 1)); // Alter 66: aktiv
        Assert.Equal(500m, state.Cash);
        state.Cash = 0m;
        state.CurrentMonthSnapshots.Clear();

        processor.ProcessMonth(project, state, new DateOnly(2021, 1, 1)); // Alter 71: inaktiv
        Assert.Equal(0m, state.Cash);
        Assert.Empty(state.CurrentMonthSnapshots);
    }
}
