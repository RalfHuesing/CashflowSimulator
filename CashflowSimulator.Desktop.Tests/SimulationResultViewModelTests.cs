using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.SimulationResult;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class SimulationResultViewModelTests
{
    [Fact]
    public async Task LoadAsync_LoadsMonthlyResultsFromService()
    {
        const long runId = 42;
        var monthly = new List<MonthlyResultDto>
        {
            new() { MonthIndex = 0, Age = 35.0, CashBalance = 10_000m, TotalAssets = 10_000m, CashflowSnapshots = [] },
            new() { MonthIndex = 1, Age = 35.08, CashBalance = 11_500m, TotalAssets = 11_500m, CashflowSnapshots = [new CashflowSnapshotEntryDto { Name = "Gehalt", CashflowType = CashflowType.Income, Amount = 1500m }] }
        };
        IResultAnalysisService service = new FakeResultAnalysisService(monthly);

        var vm = new SimulationResultViewModel(runId, service);
        Assert.Empty(vm.MonthlyResults);

        await vm.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(runId, vm.RunId);
        Assert.Equal(runId, vm.Result.RunId);
        Assert.Equal(2, vm.MonthlyResults.Count);
        Assert.Equal(0, vm.MonthlyResults[0].MonthIndex);
        Assert.Equal(10_000m, vm.MonthlyResults[0].CashBalance);
        Assert.Single(vm.MonthlyResults[1].CashflowSnapshots);
        Assert.Equal("Gehalt", vm.MonthlyResults[1].CashflowSnapshots[0].Name);
    }

    [Fact]
    public void Constructor_WithNullService_DoesNotThrow()
    {
        var vm = new SimulationResultViewModel(1, null);
        Assert.Empty(vm.MonthlyResults);
    }

    [Fact]
    public async Task LoadAsync_WithNullService_DoesNothing()
    {
        var vm = new SimulationResultViewModel(1, null);
        await vm.LoadAsync(TestContext.Current.CancellationToken);
        Assert.Empty(vm.MonthlyResults);
    }

    private sealed class FakeResultAnalysisService : IResultAnalysisService
    {
        private readonly IReadOnlyList<MonthlyResultDto> _list;

        public FakeResultAnalysisService(IReadOnlyList<MonthlyResultDto> list) => _list = list;

        public Task<IReadOnlyList<MonthlyResultDto>> GetMonthlyResultsAsync(long runId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_list);
    }
}
