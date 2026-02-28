using CashflowSimulator.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Prozessor für monatliche Cashflow-Streams (Einnahmen/Ausgaben). Aktualisiert <see cref="SimulationState.Cash"/> und füllt <see cref="SimulationState.CurrentMonthSnapshots"/>.
/// </summary>
public sealed class CashflowProcessor : ISimulationProcessor
{
    private readonly ILogger<CashflowProcessor>? _logger;

    public CashflowProcessor(ILogger<CashflowProcessor>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void ProcessMonth(SimulationProjectDto project, SimulationState state, DateOnly currentDate)
    {
        ArgumentNullException.ThrowIfNull(project.Streams);

        foreach (var stream in project.Streams)
        {
            if (!IsStreamActive(stream, currentDate))
                continue;

            if (!IsIntervalApplicable(stream.Interval, currentDate))
                continue;

            var amount = stream.Amount;
            state.CurrentMonthSnapshots.Add(new CashflowSnapshotEntryDto
            {
                Name = stream.Name,
                CashflowType = stream.Type,
                Amount = amount
            });

            switch (stream.Type)
            {
                case CashflowType.Income:
                    state.Cash += amount;
                    break;
                case CashflowType.Expense:
                    state.Cash -= amount;
                    break;
                default:
                    _logger?.LogWarning("Unbekannter Cashflow-Typ {Type} für Stream '{Name}', wird übersprungen", stream.Type, stream.Name);
                    break;
            }
        }
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
