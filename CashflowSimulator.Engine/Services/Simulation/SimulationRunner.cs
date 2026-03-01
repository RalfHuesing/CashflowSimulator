using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Führt die monatliche Simulations-Pipeline aus (Processors: Cashflow, Growth; erweiterbar um Steuern, Inflation).
/// Schreibt Ergebnisse monatlich ins <see cref="ISimulationResultRepository"/>; Rückgabe enthält nur RunId.
/// </summary>
public sealed class SimulationRunner : ISimulationRunner
{
    private readonly IEnumerable<ISimulationProcessor> _processors;
    private readonly ISimulationResultRepository _repository;

    public SimulationRunner(
        IEnumerable<ISimulationProcessor> processors,
        ISimulationResultRepository repository)
    {
        ArgumentNullException.ThrowIfNull(processors);
        ArgumentNullException.ThrowIfNull(repository);
        _processors = processors;
        _repository = repository;
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

        var runId = _repository.StartRun();

        var portfolio = project.Portfolio ?? new PortfolioDto();
        var state = new SimulationState
        {
            Cash = parameters.InitialLiquidCash,
            TotalAssets = parameters.InitialLiquidCash,
            Portfolio = new PortfolioDto
            {
                Assets = portfolio.Assets.Select(CloneAssetForSimulation).ToList(),
                Strategy = portfolio.Strategy
            }
        };

        for (var monthIndex = 0; monthIndex < monthCount; monthIndex++)
        {
            var currentDate = start.AddMonths(monthIndex);
            var age = CalculateAge(parameters.DateOfBirth, currentDate);

            foreach (var processor in _processors)
                processor.ProcessMonth(project, state, currentDate);

            var entry = new MonthlyResultDto
            {
                Age = age,
                MonthIndex = monthIndex,
                CashBalance = state.Cash,
                TotalAssets = state.TotalAssets,
                CashflowSnapshots = state.CurrentMonthSnapshots.ToList()
            };
            _repository.WriteMonthlyResult(runId, entry);
            state.CurrentMonthSnapshots.Clear();
        }

        _repository.CompleteRun(runId);
        return new SimulationResultDto { RunId = runId, MonthlyResults = [] };
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

    /// <summary>
    /// Klont ein Asset mit tiefer Kopie von Transactions und Tranches.
    /// Wenn Tranches leer sind, werden sie aus Buy-Transaktionen abgeleitet (eine Tranche pro Kauf).
    /// </summary>
    private static AssetDto CloneAssetForSimulation(AssetDto a)
    {
        var transactions = (a.Transactions ?? []).ToList();
        var tranches = (a.Tranches ?? []).ToList();
        if (tranches.Count == 0 && transactions.Count > 0)
        {
            tranches = transactions
                .Where(t => t.Type == TransactionType.Buy)
                .OrderBy(t => t.Date)
                .Select(t => new AssetTrancheDto
                {
                    PurchaseDate = t.Date,
                    Quantity = t.Quantity,
                    AcquisitionPricePerUnit = t.PricePerUnit
                })
                .ToList();
        }
        return a with
        {
            Transactions = transactions,
            Tranches = tranches
        };
    }
}
