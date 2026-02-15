using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Features.TaxProfiles;
using CashflowSimulator.Desktop.Features.StrategyProfiles;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class LifecycleProfileViewModelTests
{
    private static SimulationProjectDto ProjectWithLifecycle(
        List<TaxProfileDto>? taxProfiles = null,
        List<StrategyProfileDto>? strategyProfiles = null,
        List<LifecyclePhaseDto>? lifecyclePhases = null)
    {
        return new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "T", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [],
            TaxProfiles = taxProfiles ?? [],
            StrategyProfiles = strategyProfiles ?? [],
            LifecyclePhases = lifecyclePhases ?? [],
            UiSettings = new UiSettingsDto()
        };
    }

    [Fact]
    public void TaxProfilesViewModel_Delete_WhenPhaseReferencesProfile_ClearsTaxProfileIdInPhase()
    {
        var taxId = "tax-deleted";
        var strategyId = "strategy-1";
        var project = ProjectWithLifecycle(
            taxProfiles: [
                new TaxProfileDto { Id = taxId, Name = "Zu löschen", CapitalGainsTaxRate = 0.26m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.35m },
                new TaxProfileDto { Id = "tax-other", Name = "Andere", CapitalGainsTaxRate = 0.26m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.35m }
            ],
            strategyProfiles: [new StrategyProfileDto { Id = strategyId, Name = "S1", CashReserveMonths = 3, RebalancingThreshold = 0.05m, LookaheadMonths = 12 }],
            lifecyclePhases: [
                new LifecyclePhaseDto { StartAge = 0, TaxProfileId = taxId, StrategyProfileId = strategyId, AssetAllocationOverrides = [] },
                new LifecyclePhaseDto { StartAge = 67, TaxProfileId = "tax-other", StrategyProfileId = strategyId, AssetAllocationOverrides = [] }
            ]);
        var service = new CurrentProjectService();
        service.SetCurrent(project);

        var vm = new TaxProfilesViewModel(service, null!);
        vm.SelectedItem = project.TaxProfiles[0];
        vm.DeleteCommand.Execute(null);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.TaxProfiles);
        Assert.Equal(2, current.LifecyclePhases.Count);
        var phase0 = current.LifecyclePhases.First(p => p.StartAge == 0);
        Assert.Equal(string.Empty, phase0.TaxProfileId);
        var phase67 = current.LifecyclePhases.First(p => p.StartAge == 67);
        Assert.Equal("tax-other", phase67.TaxProfileId);
    }

    [Fact]
    public void StrategyProfilesViewModel_Delete_WhenPhaseReferencesProfile_ClearsStrategyProfileIdInPhase()
    {
        var taxId = "tax-1";
        var strategyId = "strategy-deleted";
        var project = ProjectWithLifecycle(
            taxProfiles: [new TaxProfileDto { Id = taxId, Name = "T1", CapitalGainsTaxRate = 0.26m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.35m }],
            strategyProfiles: [
                new StrategyProfileDto { Id = strategyId, Name = "Zu löschen", CashReserveMonths = 3, RebalancingThreshold = 0.05m, LookaheadMonths = 12 },
                new StrategyProfileDto { Id = "strategy-other", Name = "Andere", CashReserveMonths = 6, RebalancingThreshold = 0.05m, LookaheadMonths = 24 }
            ],
            lifecyclePhases: [
                new LifecyclePhaseDto { StartAge = 0, TaxProfileId = taxId, StrategyProfileId = strategyId, AssetAllocationOverrides = [] },
                new LifecyclePhaseDto { StartAge = 67, TaxProfileId = taxId, StrategyProfileId = "strategy-other", AssetAllocationOverrides = [] }
            ]);
        var service = new CurrentProjectService();
        service.SetCurrent(project);

        var vm = new StrategyProfilesViewModel(service, null!);
        vm.SelectedItem = project.StrategyProfiles[0];
        vm.DeleteCommand.Execute(null);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.StrategyProfiles);
        Assert.Equal(2, current.LifecyclePhases.Count);
        var phase0 = current.LifecyclePhases.First(p => p.StartAge == 0);
        Assert.Equal(string.Empty, phase0.StrategyProfileId);
        var phase67 = current.LifecyclePhases.First(p => p.StartAge == 67);
        Assert.Equal("strategy-other", phase67.StrategyProfileId);
    }
}
