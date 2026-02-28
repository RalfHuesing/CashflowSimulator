using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Features.SimulationResult;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class SimulationResultViewModelTests
{
    [Fact]
    public void Constructor_StoresResultAndExposesMonthlyResults()
    {
        var result = new SimulationResultDto
        {
            MonthlyResults =
            [
                new MonthlyResultDto { MonthIndex = 0, Age = 35.0, CashBalance = 10_000m, TotalAssets = 10_000m, CashflowSnapshots = [] },
                new MonthlyResultDto { MonthIndex = 1, Age = 35.08, CashBalance = 11_500m, TotalAssets = 11_500m, CashflowSnapshots = [new CashflowSnapshotEntryDto { Name = "Gehalt", CashflowType = CashflowType.Income, Amount = 1500m }] }
            ]
        };

        var vm = new SimulationResultViewModel(result);

        Assert.Same(result, vm.Result);
        Assert.Equal(2, vm.MonthlyResults.Count);
        Assert.Equal(0, vm.MonthlyResults[0].MonthIndex);
        Assert.Equal(10_000m, vm.MonthlyResults[0].CashBalance);
        Assert.Single(vm.MonthlyResults[1].CashflowSnapshots);
        Assert.Equal("Gehalt", vm.MonthlyResults[1].CashflowSnapshots[0].Name);
    }

    [Fact]
    public void Constructor_ThrowsWhenResultIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SimulationResultViewModel(null!));
    }
}
