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
        Assert.Equal(3, project.Streams.Count);
        Assert.Equal(2, project.Events.Count);
    }

    [Fact]
    public void CreateDefault_StreamsContainIncomeAndExpense()
    {
        var provider = new DefaultProjectProvider();
        var project = provider.CreateDefault();

        var incomeStreams = project.Streams.Where(s => s.Type == CashflowType.Income).ToList();
        var expenseStreams = project.Streams.Where(s => s.Type == CashflowType.Expense).ToList();

        Assert.Single(incomeStreams);
        Assert.Equal(2, expenseStreams.Count);
        Assert.Contains(incomeStreams, s => s.Name.Contains("Gehalt"));
    }

    [Fact]
    public void CreateDefault_EventsContainIncomeAndExpense()
    {
        var provider = new DefaultProjectProvider();
        var project = provider.CreateDefault();

        var incomeEvents = project.Events.Where(e => e.Type == CashflowType.Income).ToList();
        var expenseEvents = project.Events.Where(e => e.Type == CashflowType.Expense).ToList();

        Assert.Single(incomeEvents);
        Assert.Single(expenseEvents);
    }
}
