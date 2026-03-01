using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Infrastructure.Storage;
using Xunit;

namespace CashflowSimulator.Infrastructure.Tests;

public sealed class SqliteSimulationResultRepositoryTests
{
    private readonly SqliteSimulationResultRepository _repository = new();

    [Fact]
    public void StartRun_ReturnsPositiveRunId()
    {
        var runId = _repository.StartRun();
        Assert.True(runId > 0);
    }

    [Fact]
    public void WriteMonthlyResult_And_GetMonthlyResults_Roundtrip()
    {
        var runId = _repository.StartRun();
        var entry = new MonthlyResultDto
        {
            MonthIndex = 0,
            Age = 35.5,
            CashBalance = 10_000m,
            TotalAssets = 10_000m,
            CashflowSnapshots =
            [
                new CashflowSnapshotEntryDto { Name = "Gehalt", CashflowType = CashflowType.Income, Amount = 3000m },
                new CashflowSnapshotEntryDto { Name = "Miete", CashflowType = CashflowType.Expense, Amount = 800m }
            ]
        };
        _repository.WriteMonthlyResult(runId, entry);
        _repository.CompleteRun(runId);

        var results = _repository.GetMonthlyResults(runId);
        Assert.Single(results);
        var m = results[0];
        Assert.Equal(0, m.MonthIndex);
        Assert.Equal(35.5, m.Age);
        Assert.Equal(10_000m, m.CashBalance);
        Assert.Equal(10_000m, m.TotalAssets);
        Assert.Equal(2, m.CashflowSnapshots.Count);
        Assert.Equal("Gehalt", m.CashflowSnapshots[0].Name);
        Assert.Equal(CashflowType.Income, m.CashflowSnapshots[0].CashflowType);
        Assert.Equal(3000m, m.CashflowSnapshots[0].Amount);
        Assert.Equal("Miete", m.CashflowSnapshots[1].Name);
        Assert.Equal(CashflowType.Expense, m.CashflowSnapshots[1].CashflowType);
        Assert.Equal(800m, m.CashflowSnapshots[1].Amount);
    }

    [Fact]
    public void MultipleMonths_StoredInOrder()
    {
        var runId = _repository.StartRun();
        for (var i = 0; i < 3; i++)
        {
            _repository.WriteMonthlyResult(runId, new MonthlyResultDto
            {
                MonthIndex = i,
                Age = 34 + i * 0.08,
                CashBalance = 1000m * (i + 1),
                TotalAssets = 1000m * (i + 1),
                CashflowSnapshots = []
            });
        }
        _repository.CompleteRun(runId);

        var results = _repository.GetMonthlyResults(runId);
        Assert.Equal(3, results.Count);
        Assert.Equal(0, results[0].MonthIndex);
        Assert.Equal(1000m, results[0].CashBalance);
        Assert.Equal(1, results[1].MonthIndex);
        Assert.Equal(2000m, results[1].CashBalance);
        Assert.Equal(2, results[2].MonthIndex);
        Assert.Equal(3000m, results[2].CashBalance);
    }

    [Fact]
    public void GetMonthlyResults_UnknownRunId_ReturnsEmpty()
    {
        var results = _repository.GetMonthlyResults(999_999);
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void StartRun_ClearsPreviousRunFiles_SecondRunUsesFreshDb()
    {
        var runId1 = _repository.StartRun();
        _repository.WriteMonthlyResult(runId1, new MonthlyResultDto { MonthIndex = 0, Age = 1, CashBalance = 100m, TotalAssets = 100m });
        _repository.CompleteRun(runId1);

        var runId2 = _repository.StartRun();
        Assert.True(runId2 > 0);
        var fromRun2 = _repository.GetMonthlyResults(runId2);
        Assert.Empty(fromRun2);

        var fromRun1 = _repository.GetMonthlyResults(runId1);
        Assert.Empty(fromRun1);
    }
}
