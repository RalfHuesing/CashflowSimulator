using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Infrastructure.Storage;
using Xunit;

namespace CashflowSimulator.Infrastructure.Tests;

public sealed class SqliteSimulationResultRepositoryTests
{
    private readonly SqliteSimulationResultRepository _repository = new();

    [Fact]
    public async Task StartRunAsync_ReturnsRunStartResult_WithPositiveRunId_AndResultFolderPath()
    {
        var result = await _repository.StartRunAsync();
        Assert.True(result.RunId > 0);
        Assert.NotNull(result.ResultFolderPath);
        Assert.True(Directory.Exists(result.ResultFolderPath));
        Assert.True(File.Exists(Path.Combine(result.ResultFolderPath, "simulation.db")));
    }

    [Fact]
    public async Task WriteMonthlyResultsAsync_And_GetMonthlyResultsAsync_Roundtrip()
    {
        var start = await _repository.StartRunAsync();
        var runId = start.RunId;
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
        await _repository.WriteMonthlyResultsAsync(runId, [entry]);
        await _repository.CompleteRunAsync(runId);

        var results = await _repository.GetMonthlyResultsAsync(runId);
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
    public async Task WriteMonthlyResultsAsync_MultipleMonths_StoredInOrder()
    {
        var start = await _repository.StartRunAsync();
        var runId = start.RunId;
        var entries = new List<MonthlyResultDto>();
        for (var i = 0; i < 3; i++)
        {
            entries.Add(new MonthlyResultDto
            {
                MonthIndex = i,
                Age = 34 + i * 0.08,
                CashBalance = 1000m * (i + 1),
                TotalAssets = 1000m * (i + 1),
                CashflowSnapshots = []
            });
        }
        await _repository.WriteMonthlyResultsAsync(runId, entries);
        await _repository.CompleteRunAsync(runId);

        var results = await _repository.GetMonthlyResultsAsync(runId);
        Assert.Equal(3, results.Count);
        Assert.Equal(0, results[0].MonthIndex);
        Assert.Equal(1000m, results[0].CashBalance);
        Assert.Equal(1, results[1].MonthIndex);
        Assert.Equal(2000m, results[1].CashBalance);
        Assert.Equal(2, results[2].MonthIndex);
        Assert.Equal(3000m, results[2].CashBalance);
    }

    [Fact]
    public async Task GetMonthlyResultsAsync_UnknownRunId_ReturnsEmpty()
    {
        var results = await _repository.GetMonthlyResultsAsync(999_999);
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task StartRunAsync_TwoRunsInSequence_SecondRunHasFreshEmptyDb()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "CashflowSimulator_RepoTest_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDir);
            var repo = new SqliteSimulationResultRepository(tempDir);
            var start1 = await repo.StartRunAsync();
            await repo.WriteMonthlyResultsAsync(start1.RunId, [new MonthlyResultDto { MonthIndex = 0, Age = 1, CashBalance = 100m, TotalAssets = 100m }]);
            await repo.CompleteRunAsync(start1.RunId);

            var start2 = await repo.StartRunAsync();
            Assert.True(start2.RunId > 0);
            Assert.NotEqual(start1.RunId, start2.RunId);
            var fromRun2 = await repo.GetMonthlyResultsAsync(start2.RunId);
            Assert.Empty(fromRun2);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
            catch { /* cleanup best-effort */ }
        }
    }

    [Fact]
    public async Task WriteMonthlyResultsAsync_LargeBatch_360Months_Roundtrip()
    {
        var start = await _repository.StartRunAsync();
        var runId = start.RunId;
        var entries = new List<MonthlyResultDto>(360);
        for (var i = 0; i < 360; i++)
        {
            entries.Add(new MonthlyResultDto
            {
                MonthIndex = i,
                Age = 30 + i / 12.0,
                CashBalance = 50_000m + i * 100m,
                TotalAssets = 100_000m + i * 200m,
                CashflowSnapshots = i % 12 == 0 ? [new CashflowSnapshotEntryDto { Name = "Bonus", CashflowType = CashflowType.Income, Amount = 5000m }] : []
            });
        }
        await _repository.WriteMonthlyResultsAsync(runId, entries);
        await _repository.CompleteRunAsync(runId);

        var results = await _repository.GetMonthlyResultsAsync(runId);
        Assert.Equal(360, results.Count);
        Assert.Equal(0, results[0].MonthIndex);
        Assert.Equal(359, results[359].MonthIndex);
        Assert.Equal(50_000m, results[0].CashBalance);
        Assert.Equal(50_000m + 359 * 100m, results[359].CashBalance);
        Assert.Single(results[0].CashflowSnapshots);
        Assert.Empty(results[1].CashflowSnapshots);
    }
}
