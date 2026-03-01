using System.Collections.Concurrent;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Tests;

/// <summary>
/// In-Memory-Implementierung für Tests: speichert Monatsergebnisse pro RunId (async API).
/// </summary>
internal sealed class InMemorySimulationResultRepository : ISimulationResultRepository
{
    private long _nextRunId;
    private readonly ConcurrentDictionary<long, List<MonthlyResultDto>> _runData = new();

    public Task<long> StartRunAsync(CancellationToken cancellationToken = default)
    {
        var runId = Interlocked.Increment(ref _nextRunId);
        _runData[runId] = [];
        return Task.FromResult(runId);
    }

    public Task WriteMonthlyResultsAsync(long runId, IEnumerable<MonthlyResultDto> entries, CancellationToken cancellationToken = default)
    {
        if (!_runData.TryGetValue(runId, out var list))
            throw new InvalidOperationException($"Run {runId} nicht gefunden.");
        foreach (var entry in entries)
            list.Add(entry);
        return Task.CompletedTask;
    }

    public Task CompleteRunAsync(long runId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<MonthlyResultDto>> GetMonthlyResultsAsync(long runId, CancellationToken cancellationToken = default)
    {
        var result = _runData.TryGetValue(runId, out var list) ? (IReadOnlyList<MonthlyResultDto>)list : [];
        return Task.FromResult(result);
    }
}
