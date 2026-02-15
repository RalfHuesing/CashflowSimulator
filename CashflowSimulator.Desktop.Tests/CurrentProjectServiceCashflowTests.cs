using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class CurrentProjectServiceCashflowTests
{
    [Fact]
    public void UpdateStreams_ReplacesStreamsInCurrentProject()
    {
        var service = new CurrentProjectService();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [new CashflowStreamDto { Id = "old", Name = "Alt", Type = CashflowType.Income, Amount = 100, StartDate = DateOnly.FromDateTime(DateTime.Today) }],
            Events = [],
            UiSettings = new UiSettingsDto()
        };
        service.SetCurrent(project);

        var newStreams = new List<CashflowStreamDto>
        {
            new() { Id = "new1", Name = "Neu1", Type = CashflowType.Income, Amount = 200, StartDate = DateOnly.FromDateTime(DateTime.Today) }
        };
        service.UpdateStreams(newStreams);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.Streams);
        Assert.Equal("Neu1", current.Streams[0].Name);
        Assert.Equal(200, current.Streams[0].Amount);
    }

    [Fact]
    public void UpdateEvents_ReplacesEventsInCurrentProject()
    {
        var service = new CurrentProjectService();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [new CashflowEventDto { Id = "old", Name = "Alt", Type = CashflowType.Expense, Amount = 500, TargetDate = DateOnly.FromDateTime(DateTime.Today) }],
            UiSettings = new UiSettingsDto()
        };
        service.SetCurrent(project);

        var newEvents = new List<CashflowEventDto>
        {
            new() { Id = "new1", Name = "NeuEvent", Type = CashflowType.Expense, Amount = 1000, TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)) }
        };
        service.UpdateEvents(newEvents);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.Events);
        Assert.Equal("NeuEvent", current.Events[0].Name);
        Assert.Equal(1000, current.Events[0].Amount);
    }

    [Fact]
    public void UpdateTaxProfiles_ReplacesTaxProfilesInCurrentProject()
    {
        var service = new CurrentProjectService();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [],
            TaxProfiles = [new TaxProfileDto { Id = "t1", Name = "Alt", CapitalGainsTaxRate = 0.26m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.35m }],
            StrategyProfiles = [],
            LifecyclePhases = [],
            UiSettings = new UiSettingsDto()
        };
        service.SetCurrent(project);

        var newProfiles = new List<TaxProfileDto>
        {
            new() { Id = "t2", Name = "Neu", CapitalGainsTaxRate = 0.26375m, TaxFreeAllowance = 801m, IncomeTaxRate = 0.18m }
        };
        service.UpdateTaxProfiles(newProfiles);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.TaxProfiles);
        Assert.Equal("Neu", current.TaxProfiles[0].Name);
        Assert.Equal(801m, current.TaxProfiles[0].TaxFreeAllowance);
    }

    [Fact]
    public void UpdateStrategyProfiles_ReplacesStrategyProfilesInCurrentProject()
    {
        var service = new CurrentProjectService();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [],
            TaxProfiles = [],
            StrategyProfiles = [new StrategyProfileDto { Id = "s1", Name = "Alt", CashReserveMonths = 3, RebalancingThreshold = 0.05m, LookaheadMonths = 12 }],
            LifecyclePhases = [],
            UiSettings = new UiSettingsDto()
        };
        service.SetCurrent(project);

        var newProfiles = new List<StrategyProfileDto>
        {
            new() { Id = "s2", Name = "Neu", CashReserveMonths = 6, RebalancingThreshold = 0.1m, LookaheadMonths = 24 }
        };
        service.UpdateStrategyProfiles(newProfiles);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.StrategyProfiles);
        Assert.Equal("Neu", current.StrategyProfiles[0].Name);
        Assert.Equal(24, current.StrategyProfiles[0].LookaheadMonths);
    }

    [Fact]
    public void UpdateLifecyclePhases_ReplacesLifecyclePhasesInCurrentProject()
    {
        var service = new CurrentProjectService();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [],
            TaxProfiles = [new TaxProfileDto { Id = "t1", Name = "T", CapitalGainsTaxRate = 0.26m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.35m }],
            StrategyProfiles = [new StrategyProfileDto { Id = "s1", Name = "S", CashReserveMonths = 3, RebalancingThreshold = 0.05m, LookaheadMonths = 12 }],
            LifecyclePhases = [new LifecyclePhaseDto { StartAge = 0, TaxProfileId = "t1", StrategyProfileId = "s1", AssetAllocationOverrides = [] }],
            UiSettings = new UiSettingsDto()
        };
        service.SetCurrent(project);

        var newPhases = new List<LifecyclePhaseDto>
        {
            new() { StartAge = 67, TaxProfileId = "t1", StrategyProfileId = "s1", AssetAllocationOverrides = [] }
        };
        service.UpdateLifecyclePhases(newPhases);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.LifecyclePhases);
        Assert.Equal(67, current.LifecyclePhases[0].StartAge);
    }
}
