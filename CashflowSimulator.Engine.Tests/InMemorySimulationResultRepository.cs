using System.Collections.Concurrent;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Tests;

/// <summary>
/// In-Memory-Implementierung für Tests: speichert Monatsergebnisse pro RunId.
/// </summary>
internal sealed class InMemorySimulationResultRepository : ISimulationResultRepository
{
    private long _nextRunId;
    private readonly ConcurrentDictionary<long, List<MonthlyResultDto>> _runData = new();

    public long StartRun()
    {
        var runId = Interlocked.Increment(ref _nextRunId);
        _runData[runId] = [];
        return runId;
    }

    public void WriteMonthlyResult(long runId, MonthlyResultDto entry)
    {
        if (!_runData.TryGetValue(runId, out var list))
            throw new InvalidOperationException($"Run {runId} nicht gefunden.");
        list.Add(entry);
    }

    public void CompleteRun(long runId) { }

    public IReadOnlyList<MonthlyResultDto> GetMonthlyResults(long runId) =>
        _runData.TryGetValue(runId, out var list) ? list : [];
}
