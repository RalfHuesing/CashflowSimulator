using CashflowSimulator.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Prozessor für Kaufkraftverlust und Gehaltsdynamik: wendet in jedem Simulationsmonat die Inflationsraten
/// aus <see cref="EconomicFactors"/> monatlich geglättet (stetige Verzinsung e^(rate/12)) auf die indexierten
/// Stream-Beträge in <see cref="SimulationState.IndexedStreamAmounts"/> an.
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

            var rate = factor.ExpectedReturn;
            var monthlyFactor = (decimal)Math.Exp(rate / 12.0);
            state.IndexedStreamAmounts[stream.Id] = currentAmount * monthlyFactor;
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
