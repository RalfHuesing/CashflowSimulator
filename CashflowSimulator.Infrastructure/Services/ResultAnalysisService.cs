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
    public IReadOnlyList<Contracts.Dtos.MonthlyResultDto> GetMonthlyResults(long runId) =>
        _repository.GetMonthlyResults(runId);
}
