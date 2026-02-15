using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class DefaultProjectProviderTests
{
    [Fact]
    public void CreateDefault_ReturnsProjectWithStreamsAndEvents()
    {
        var provider = new DefaultProjectProvider();
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
        var provider = new DefaultProjectProvider();
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
        var provider = new DefaultProjectProvider();
        var project = provider.CreateDefault();

        var incomeEvents = project.Events.Where(e => e.Type == CashflowType.Income).ToList();
        var expenseEvents = project.Events.Where(e => e.Type == CashflowType.Expense).ToList();

        Assert.Single(incomeEvents);
        Assert.Equal(5, expenseEvents.Count);
    }
}
