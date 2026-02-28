using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Führt die monatliche Simulations-Pipeline aus (Slice 1: nur Cashflow; keine Steuern, keine Inflation, kein Depot).
/// </summary>
public sealed class SimulationRunner : ISimulationRunner
{
    /// <inheritdoc />
    public SimulationResultDto RunSimulation(SimulationProjectDto project)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(project.Parameters);
        ArgumentNullException.ThrowIfNull(project.Streams);

        var parameters = project.Parameters;
        var start = parameters.SimulationStart;
        var end = parameters.SimulationEnd;
        var monthCount = GetMonthCount(start, end);

        if (monthCount <= 0)
            return new SimulationResultDto { MonthlyResults = [] };

        var monthlyResults = new List<MonthlyResultDto>(monthCount);
        var state = new SimulationState
        {
            Cash = parameters.InitialLiquidCash,
            TotalAssets = parameters.InitialLiquidCash
        };

        for (var monthIndex = 0; monthIndex < monthCount; monthIndex++)
        {
            var currentDate = start.AddMonths(monthIndex);
            var age = CalculateAge(parameters.DateOfBirth, currentDate);

            var snapshots = ProcessCashflowStreams(project.Streams, currentDate, state);
            state.TotalAssets = state.Cash;

            monthlyResults.Add(new MonthlyResultDto
            {
                Age = age,
                MonthIndex = monthIndex,
                CashBalance = state.Cash,
                TotalAssets = state.TotalAssets,
                CashflowSnapshots = snapshots
            });
        }

        return new SimulationResultDto { MonthlyResults = monthlyResults };
    }

    private static int GetMonthCount(DateOnly start, DateOnly end)
    {
        var months = ((end.Year - start.Year) * 12) + (end.Month - start.Month) + 1;
        return Math.Max(0, months);
    }

    private static double CalculateAge(DateOnly dateOfBirth, DateOnly currentDate)
    {
        var totalDays = (currentDate.ToDateTime(TimeOnly.MinValue) - dateOfBirth.ToDateTime(TimeOnly.MinValue)).TotalDays;
        return totalDays / 365.25;
    }

    private static List<CashflowSnapshotEntryDto> ProcessCashflowStreams(
        List<CashflowStreamDto> streams,
        DateOnly currentDate,
        SimulationState state)
    {
        var snapshots = new List<CashflowSnapshotEntryDto>();

        foreach (var stream in streams)
        {
            if (!IsStreamActive(stream, currentDate))
                continue;

            if (!IsIntervalApplicable(stream.Interval, currentDate))
                continue;

            var amount = stream.Amount;
            snapshots.Add(new CashflowSnapshotEntryDto
            {
                Name = stream.Name,
                CashflowType = stream.Type,
                Amount = amount
            });

            if (stream.Type == CashflowType.Income)
                state.Cash += amount;
            else
                state.Cash -= amount;
        }

        return snapshots;
    }

    private static bool IsStreamActive(CashflowStreamDto stream, DateOnly currentDate)
    {
        if (stream.StartDate > currentDate)
            return false;
        if (stream.EndDate is null)
            return true;
        return currentDate <= stream.EndDate.Value;
    }

    private static bool IsIntervalApplicable(CashflowInterval interval, DateOnly currentDate)
    {
        return interval switch
        {
            CashflowInterval.Monthly => true,
            CashflowInterval.Yearly => currentDate.Month == 1,
            _ => true
        };
    }
}
