using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Unit-Tests für <see cref="TaxContext"/>: Verlustvorträge aus Parameters übernommen, null Parameters wirft.
/// </summary>
public sealed class TaxContextTests
{
    [Fact]
    public void Constructor_CopiesInitialLossCarryforwardFromParameters()
    {
        var parameters = new SimulationParametersDto
        {
            SimulationStart = new DateOnly(2020, 1, 1),
            SimulationEnd = new DateOnly(2070, 1, 1),
            DateOfBirth = new DateOnly(1980, 1, 1),
            InitialLossCarryforwardGeneral = 1500m,
            InitialLossCarryforwardStocks = 800m,
            InitialLiquidCash = 0,
            CurrencyCode = "EUR"
        };

        var context = new TaxContext(parameters);

        Assert.Equal(1500m, context.LossCarryforwardGeneral);
        Assert.Equal(800m, context.LossCarryforwardStocks);
    }

    [Fact]
    public void Constructor_ZeroLossCarryforwards_InitializesToZero()
    {
        var parameters = new SimulationParametersDto
        {
            SimulationStart = new DateOnly(2020, 1, 1),
            SimulationEnd = new DateOnly(2070, 1, 1),
            DateOfBirth = new DateOnly(1980, 1, 1),
            InitialLossCarryforwardGeneral = 0,
            InitialLossCarryforwardStocks = 0,
            InitialLiquidCash = 0,
            CurrencyCode = "EUR"
        };

        var context = new TaxContext(parameters);

        Assert.Equal(0m, context.LossCarryforwardGeneral);
        Assert.Equal(0m, context.LossCarryforwardStocks);
    }

    [Fact]
    public void Constructor_NullParameters_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new TaxContext(null!));

        Assert.Equal("parameters", ex.ParamName);
    }
}
