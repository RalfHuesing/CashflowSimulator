using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Führt die monatliche Simulations-Pipeline aus (Processors: Cashflow, Growth; erweiterbar um Steuern, Inflation).
/// </summary>
public sealed class SimulationRunner : ISimulationRunner
{
    private readonly IEnumerable<ISimulationProcessor> _processors;

    public SimulationRunner(IEnumerable<ISimulationProcessor> processors)
    {
        ArgumentNullException.ThrowIfNull(processors);
        _processors = processors;
    }

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

        var portfolio = project.Portfolio ?? new PortfolioDto();
        var state = new SimulationState
        {
            Cash = parameters.InitialLiquidCash,
            TotalAssets = parameters.InitialLiquidCash,
            Portfolio = new PortfolioDto
            {
                Assets = portfolio.Assets.Select(a => a with { }).ToList(),
                Strategy = portfolio.Strategy
            }
        };

        var monthlyResults = new List<MonthlyResultDto>(monthCount);

        for (var monthIndex = 0; monthIndex < monthCount; monthIndex++)
        {
            var currentDate = start.AddMonths(monthIndex);
            var age = CalculateAge(parameters.DateOfBirth, currentDate);

            foreach (var processor in _processors)
                processor.ProcessMonth(project, state, currentDate);

            monthlyResults.Add(new MonthlyResultDto
            {
                Age = age,
                MonthIndex = monthIndex,
                CashBalance = state.Cash,
                TotalAssets = state.TotalAssets,
                CashflowSnapshots = state.CurrentMonthSnapshots.ToList()
            });
            state.CurrentMonthSnapshots.Clear();
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
}
