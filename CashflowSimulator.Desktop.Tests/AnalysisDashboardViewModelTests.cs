using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Analysis;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class AnalysisDashboardViewModelTests
{
    [Fact]
    public async Task LoadAsync_WithRunIdZero_SetsIsEmpty()
    {
        var logger = NullLogger<AnalysisDashboardViewModel>.Instance;
        var vm = new AnalysisDashboardViewModel(0, null, logger);
        await vm.LoadAsync();
        Assert.True(vm.IsEmpty);
        Assert.Empty(vm.YearlySummaries);
        Assert.Empty(vm.TotalAssetsSeries);
    }

    [Fact]
    public async Task LoadAsync_WithNullService_SetsIsEmpty()
    {
        var logger = NullLogger<AnalysisDashboardViewModel>.Instance;
        var vm = new AnalysisDashboardViewModel(1, null, logger);
        await vm.LoadAsync();
        Assert.True(vm.IsEmpty);
    }

    [Fact]
    public async Task LoadAsync_WithData_BuildsYearlySummariesAndChart()
    {
        var monthly = new List<MonthlyResultDto>();
        for (var i = 0; i < 25; i++)
            monthly.Add(new MonthlyResultDto
            {
                MonthIndex = i,
                Age = 35 + i / 12.0,
                CashBalance = 10_000m + i * 100m,
                TotalAssets = 10_000m + i * 100m,
                CashflowSnapshots = []
            });
        IResultAnalysisService service = new FakeResultAnalysisService(monthly);
        var logger = NullLogger<AnalysisDashboardViewModel>.Instance;

        var vm = new AnalysisDashboardViewModel(42, service, logger);
        await vm.LoadAsync();

        Assert.False(vm.IsEmpty);
        Assert.Equal(42, vm.RunId);
        Assert.Equal(3, vm.YearlySummaries.Count);
        Assert.Equal(0, vm.YearlySummaries[0].YearIndex);
        Assert.Equal(11_100m, vm.YearlySummaries[0].EndTotalAssets);
        Assert.Equal(2, vm.YearlySummaries[2].YearIndex);
        Assert.Equal(12_400m, vm.YearlySummaries[2].EndTotalAssets);
        Assert.Single(vm.TotalAssetsSeries);
    }

    [Fact]
    public async Task SelectedYearlySummary_UpdatesMonthsInSelectedYear()
    {
        var monthly = new List<MonthlyResultDto>();
        for (var i = 0; i < 14; i++)
            monthly.Add(new MonthlyResultDto { MonthIndex = i, Age = 35.0, CashBalance = 100m, TotalAssets = 100m, CashflowSnapshots = [] });
        IResultAnalysisService service = new FakeResultAnalysisService(monthly);
        var logger = NullLogger<AnalysisDashboardViewModel>.Instance;

        var vm = new AnalysisDashboardViewModel(1, service, logger);
        await vm.LoadAsync();

        vm.SelectedYearlySummary = vm.YearlySummaries[0];
        Assert.Equal(12, vm.MonthsInSelectedYear.Count);
        Assert.Equal(0, vm.MonthsInSelectedYear[0].MonthIndex);
        Assert.Equal(11, vm.MonthsInSelectedYear[11].MonthIndex);

        vm.SelectedYearlySummary = vm.YearlySummaries[1];
        Assert.Equal(2, vm.MonthsInSelectedYear.Count);
        Assert.Equal(12, vm.MonthsInSelectedYear[0].MonthIndex);
    }

    private sealed class FakeResultAnalysisService : IResultAnalysisService
    {
        private readonly IReadOnlyList<MonthlyResultDto> _list;

        public FakeResultAnalysisService(IReadOnlyList<MonthlyResultDto> list) => _list = list;

        public Task<IReadOnlyList<MonthlyResultDto>> GetMonthlyResultsAsync(long runId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_list);
    }
}
