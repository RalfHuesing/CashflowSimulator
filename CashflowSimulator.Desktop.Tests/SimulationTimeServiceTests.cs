using CashflowSimulator.Engine.Services.Defaults;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class SimulationTimeServiceTests
{
    [Fact]
    public void GetDefaultTimeContext_ReturnsParametersWithExpectedStructure()
    {
        var service = new SimulationTimeService();
        var context = service.GetDefaultTimeContext();

        Assert.NotEqual(default, context.Now);
        Assert.NotNull(context.Parameters);
        var p = context.Parameters;
        Assert.NotEqual(default, p.SimulationStart);
        Assert.NotEqual(default, p.SimulationEnd);
        Assert.NotEqual(default, p.DateOfBirth);
    }

    [Fact]
    public void GetDefaultTimeContext_SimulationEndBasedOnLifeExpectancy90()
    {
        var service = new SimulationTimeService();
        var context = service.GetDefaultTimeContext();

        var p = context.Parameters;
        var birthYear = p.DateOfBirth.Year;
        Assert.Equal(birthYear + 90, p.SimulationEnd.Year);
    }

    [Fact]
    public void GetDefaultTimeContext_ReturnsEuroAndInitialCash()
    {
        var service = new SimulationTimeService();
        var context = service.GetDefaultTimeContext();

        Assert.Equal("EUR", context.Parameters.CurrencyCode);
        Assert.Equal(15_000m, context.Parameters.InitialLiquidCash);
    }
}
