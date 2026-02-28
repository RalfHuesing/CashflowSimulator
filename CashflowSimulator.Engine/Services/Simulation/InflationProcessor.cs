using CashflowSimulator.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Prozessor für Kaufkraftverlust und Gehaltsdynamik: wendet zum Jahreswechsel (Monat 1) die Inflationsraten
/// aus <see cref="EconomicFactors"/> auf die indexierten Stream-Beträge in <see cref="SimulationState.IndexedStreamAmounts"/> an.
/// Das Projekt-DTO bleibt unverändert; der CashflowProcessor liest die angepassten Beträge aus dem State.
/// </summary>
public sealed class InflationProcessor : ISimulationProcessor
{
    private readonly ILogger<InflationProcessor>? _logger;

    public InflationProcessor(ILogger<InflationProcessor>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void ProcessMonth(SimulationProjectDto project, SimulationState state, DateOnly currentDate)
    {
        ArgumentNullException.ThrowIfNull(project.Streams);

        EnsureIndexedStreamAmountsInitialized(project, state);

        if (currentDate.Month != 1)
            return;

        var parameters = project.Parameters;
        var simStart = parameters?.SimulationStart ?? default;
        if (simStart == default || currentDate <= simStart)
            return;

        var factors = project.EconomicFactors ?? [];
        foreach (var stream in project.Streams)
        {
            if (string.IsNullOrEmpty(stream.EconomicFactorId))
                continue;

            if (!state.IndexedStreamAmounts.TryGetValue(stream.Id, out var currentAmount))
                continue;

            var factor = factors.FirstOrDefault(f => f.Id == stream.EconomicFactorId);
            if (factor == null)
            {
                _logger?.LogWarning(
                    "Kein EconomicFactor mit Id '{FactorId}' für Stream '{StreamName}' gefunden, Betrag unverändert",
                    stream.EconomicFactorId, stream.Name);
                continue;
            }

            var rate = (decimal)factor.ExpectedReturn;
            var newAmount = currentAmount * (1 + rate);
            state.IndexedStreamAmounts[stream.Id] = newAmount;
        }
    }

    private static void EnsureIndexedStreamAmountsInitialized(SimulationProjectDto project, SimulationState state)
    {
        if (state.IndexedStreamAmounts.Count > 0)
            return;

        foreach (var stream in project.Streams)
            state.IndexedStreamAmounts[stream.Id] = stream.Amount;
    }
}
