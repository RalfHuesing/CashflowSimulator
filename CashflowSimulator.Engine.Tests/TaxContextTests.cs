using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

/// <summary>
/// Unit-Tests für <see cref="TaxContext"/>: Verlustvorträge aus Parameters übernommen, Verrechnung mit Gewinnen, null Parameters wirft.
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

    // --- Golden Master: Verrechnung allgemeiner Verlusttopf ---

    [Fact]
    public void ApplyGeneralGain_PartialOffset_ReducesCarryforward()
    {
        var parameters = new SimulationParametersDto
        {
            SimulationStart = default,
            SimulationEnd = default,
            DateOfBirth = default,
            InitialLossCarryforwardGeneral = 5000m,
            InitialLossCarryforwardStocks = 0,
            InitialLiquidCash = 0,
            CurrencyCode = "EUR"
        };
        var context = new TaxContext(parameters);

        context.ApplyGeneralGain(3000m);

        Assert.Equal(2000m, context.LossCarryforwardGeneral);
        Assert.Equal(0m, context.LossCarryforwardStocks);
    }

    [Fact]
    public void ApplyGeneralGain_FullOffset_ZeroCarryforward()
    {
        var parameters = new SimulationParametersDto
        {
            InitialLossCarryforwardGeneral = 5000m,
            InitialLossCarryforwardStocks = 0
        };
        var context = new TaxContext(parameters);

        context.ApplyGeneralGain(5000m);

        Assert.Equal(0m, context.LossCarryforwardGeneral);
    }

    [Fact]
    public void ApplyGeneralGain_OverOffset_FloorZero()
    {
        var parameters = new SimulationParametersDto
        {
            InitialLossCarryforwardGeneral = 5000m,
            InitialLossCarryforwardStocks = 0
        };
        var context = new TaxContext(parameters);

        context.ApplyGeneralGain(7000m);

        Assert.Equal(0m, context.LossCarryforwardGeneral);
    }

    [Fact]
    public void ApplyGeneralGain_ZeroOrNegative_NoChange()
    {
        var parameters = new SimulationParametersDto
        {
            InitialLossCarryforwardGeneral = 1000m,
            InitialLossCarryforwardStocks = 0
        };
        var context = new TaxContext(parameters);

        context.ApplyGeneralGain(0m);
        Assert.Equal(1000m, context.LossCarryforwardGeneral);

        context.ApplyGeneralGain(-100m);
        Assert.Equal(1000m, context.LossCarryforwardGeneral);
    }

    // --- Golden Master: Verrechnung Aktienverlusttopf ---

    [Fact]
    public void ApplyStocksGain_PartialOffset_ReducesCarryforward()
    {
        var parameters = new SimulationParametersDto
        {
            InitialLossCarryforwardGeneral = 0,
            InitialLossCarryforwardStocks = 3000m
        };
        var context = new TaxContext(parameters);

        context.ApplyStocksGain(1000m);

        Assert.Equal(0m, context.LossCarryforwardGeneral);
        Assert.Equal(2000m, context.LossCarryforwardStocks);
    }

    [Fact]
    public void ApplyStocksGain_OverOffset_FloorZero()
    {
        var parameters = new SimulationParametersDto
        {
            InitialLossCarryforwardGeneral = 0,
            InitialLossCarryforwardStocks = 2000m
        };
        var context = new TaxContext(parameters);

        context.ApplyStocksGain(5000m);

        Assert.Equal(0m, context.LossCarryforwardStocks);
    }

    // --- Golden Master: Kombination beider Töpfe ---

    [Fact]
    public void ApplyBothGains_GeneralThenStocks_ReducesBothCorrectly()
    {
        var parameters = new SimulationParametersDto
        {
            InitialLossCarryforwardGeneral = 4000m,
            InitialLossCarryforwardStocks = 1500m
        };
        var context = new TaxContext(parameters);

        context.ApplyGeneralGain(2000m);
        context.ApplyStocksGain(800m);

        Assert.Equal(2000m, context.LossCarryforwardGeneral);
        Assert.Equal(700m, context.LossCarryforwardStocks);
    }

    // --- Jahreswechsel: Verlustvortrag persistiert (kein Reset) ---

    [Fact]
    public void CarryforwardPersists_AcrossMultipleApplyCalls_NoReset()
    {
        var parameters = new SimulationParametersDto
        {
            InitialLossCarryforwardGeneral = 10000m,
            InitialLossCarryforwardStocks = 5000m
        };
        var context = new TaxContext(parameters);

        context.ApplyGeneralGain(3000m);
        Assert.Equal(7000m, context.LossCarryforwardGeneral);

        context.ApplyGeneralGain(2000m);
        Assert.Equal(5000m, context.LossCarryforwardGeneral);

        context.ApplyStocksGain(2000m);
        Assert.Equal(3000m, context.LossCarryforwardStocks);
    }
}
