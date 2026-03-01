using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Infrastructure.Services;

/// <summary>
/// Delegiert Leseanfragen an das Simulationsergebnis-Repository.
/// </summary>
public sealed class ResultAnalysisService : IResultAnalysisService
{
    private readonly ISimulationResultRepository _repository;

    public ResultAnalysisService(ISimulationResultRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Contracts.Dtos.MonthlyResultDto>> GetMonthlyResultsAsync(long runId, CancellationToken cancellationToken = default) =>
        await _repository.GetMonthlyResultsAsync(runId, cancellationToken).ConfigureAwait(false);
}
