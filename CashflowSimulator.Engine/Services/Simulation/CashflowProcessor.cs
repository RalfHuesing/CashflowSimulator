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

        var dateOfBirth = project.Parameters?.DateOfBirth;
        foreach (var stream in project.Streams)
        {
            if (!IsStreamActive(stream, currentDate, dateOfBirth))
                continue;

            if (!IsIntervalApplicable(stream.Interval, currentDate))
                continue;

            var amount = state.IndexedStreamAmounts.TryGetValue(stream.Id, out var indexedAmount)
                ? indexedAmount
                : stream.Amount;
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

    private static bool IsStreamActive(CashflowStreamDto stream, DateOnly currentDate, DateOnly? dateOfBirth)
    {
        if (stream.StartAge is int startAge || stream.EndAge is int endAge)
        {
            if (!dateOfBirth.HasValue || dateOfBirth.Value == default)
                return false;
            var ageInYears = (int)((currentDate.ToDateTime(TimeOnly.MinValue) - dateOfBirth.Value.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25);
            if (stream.StartAge is int sa && ageInYears < sa)
                return false;
            if (stream.EndAge is int ea && ageInYears > ea)
                return false;
        }
        if (!stream.StartAge.HasValue && stream.StartDate > currentDate)
            return false;
        if (!stream.EndAge.HasValue)
        {
            if (stream.EndDate is null)
                return true;
            if (currentDate > stream.EndDate.Value)
                return false;
        }
        return true;
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
