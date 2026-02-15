using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Engine.Services;
using CashflowSimulator.Engine.Services.Defaults;
using CashflowSimulator.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class DefaultProjectProviderTests
{
    private static IDefaultProjectProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISimulationTimeService, SimulationTimeService>();
        services.AddSingleton<IMarketDataService, MarketDataService>();
        services.AddSingleton<ICashflowDefaultService, CashflowDefaultService>();
        services.AddSingleton<IPortfolioDefaultService, PortfolioDefaultService>();
        services.AddSingleton<IDefaultProjectProvider, DefaultProjectProvider>();
        return services.BuildServiceProvider().GetRequiredService<IDefaultProjectProvider>();
    }

    [Fact]
    public void CreateDefault_ReturnsValidSimulationProjectDto()
    {
        var provider = CreateProvider();
        var project = provider.CreateDefault();

        Assert.NotNull(project);
        Assert.NotNull(project.Meta);
        Assert.NotNull(project.Parameters);
        Assert.NotNull(project.Streams);
        Assert.NotNull(project.Events);
        Assert.NotNull(project.EconomicFactors);
        Assert.NotNull(project.Correlations);
        Assert.NotNull(project.AssetClasses);
        Assert.NotNull(project.TaxProfiles);
        Assert.NotNull(project.StrategyProfiles);
        Assert.NotNull(project.LifecyclePhases);
        Assert.NotNull(project.Portfolio);
        Assert.NotNull(project.Portfolio.Assets);
        Assert.NotEqual(default, project.Parameters.SimulationStart);
        Assert.NotEqual(default, project.Parameters.SimulationEnd);
    }

    [Fact]
    public void CreateDefault_ReturnsProjectWithLifecyclePhasesAndReferenceIntegrity()
    {
        var provider = CreateProvider();
        var project = provider.CreateDefault();

        Assert.NotNull(project.TaxProfiles);
        Assert.NotNull(project.StrategyProfiles);
        Assert.NotNull(project.LifecyclePhases);
        Assert.Equal(2, project.TaxProfiles.Count);
        Assert.Equal(2, project.StrategyProfiles.Count);
        Assert.Equal(2, project.LifecyclePhases.Count);

        var taxIds = project.TaxProfiles.Select(p => p.Id).ToHashSet();
        var strategyIds = project.StrategyProfiles.Select(p => p.Id).ToHashSet();
        foreach (var phase in project.LifecyclePhases)
        {
            Assert.Contains(phase.TaxProfileId, taxIds);
            Assert.Contains(phase.StrategyProfileId, strategyIds);
        }

        var validationResult = ValidationRunner.Validate(project);
        Assert.True(validationResult.IsValid, "Default-Projekt muss die Projekt-Validierung bestehen. Fehler: " + string.Join("; ", validationResult.Errors.Select(e => e.PropertyName + ": " + e.Message)));

        foreach (var phase in project.LifecyclePhases)
            Assert.False(string.IsNullOrEmpty(phase.Id), "Jede LifecyclePhase muss eine gültige Id haben.");
        foreach (var asset in project.Portfolio.Assets)
        {
            foreach (var tx in asset.Transactions)
                Assert.False(string.IsNullOrEmpty(tx.Id), "Jede Transaktion muss eine gültige Id haben.");
        }
    }

    [Fact]
    public void CreateDefault_ReturnsProjectWithStreamsAndEvents()
    {
        var provider = CreateProvider();
        var project = provider.CreateDefault();

        Assert.NotNull(project.Streams);
        Assert.NotNull(project.Events);
        Assert.NotNull(project.EconomicFactors);
        Assert.NotNull(project.Correlations);
        Assert.NotNull(project.AssetClasses);
        Assert.Equal(10, project.Streams.Count);
        Assert.Equal(6, project.Events.Count);
        Assert.Equal(4, project.EconomicFactors.Count);
        Assert.Equal(3, project.AssetClasses.Count);
        Assert.True(project.Correlations.Count >= 1);
    }

    [Fact]
    public void CreateDefault_StreamsContainIncomeAndExpense()
    {
        var provider = CreateProvider();
        var project = provider.CreateDefault();

        var incomeStreams = project.Streams.Where(s => s.Type == CashflowType.Income).ToList();
        var expenseStreams = project.Streams.Where(s => s.Type == CashflowType.Expense).ToList();

        Assert.Equal(2, incomeStreams.Count);
        Assert.Equal(8, expenseStreams.Count);
        Assert.Contains(incomeStreams, s => s.Name.Contains("Gehalt"));
        Assert.Contains(incomeStreams, s => s.Name.Contains("Rente"));
    }

    [Fact]
    public void CreateDefault_EventsContainIncomeAndExpense()
    {
        var provider = CreateProvider();
        var project = provider.CreateDefault();

        var incomeEvents = project.Events.Where(e => e.Type == CashflowType.Income).ToList();
        var expenseEvents = project.Events.Where(e => e.Type == CashflowType.Expense).ToList();

        Assert.Single(incomeEvents);
        Assert.Equal(5, expenseEvents.Count);
    }
}
