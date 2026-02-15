using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services.Rebalancing;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Unit-Tests für <see cref="RebalancingService.ShouldGenerateOrder"/>:
/// Grenzfälle Mindest-Transaktionsgröße, positive/negative Differenz, null Strategy.
/// </summary>
public sealed class RebalancingServiceTests
{
    private static StrategyProfileDto Strategy(decimal minimumTransactionAmount = 50m) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Test",
        CashReserveMonths = 3,
        RebalancingThreshold = 0.05m,
        MinimumTransactionAmount = minimumTransactionAmount,
        LookaheadMonths = 24
    };

    [Fact]
    public void ShouldGenerateOrder_AmountEqualsMinimum_ReturnsTrue()
    {
        var sut = new RebalancingService();
        var strategy = Strategy(50m);

        Assert.True(sut.ShouldGenerateOrder(50m, strategy));
        Assert.True(sut.ShouldGenerateOrder(-50m, strategy));
    }

    [Fact]
    public void ShouldGenerateOrder_AmountBelowMinimum_ReturnsFalse()
    {
        var sut = new RebalancingService();
        var strategy = Strategy(50m);

        Assert.False(sut.ShouldGenerateOrder(49m, strategy));
        Assert.False(sut.ShouldGenerateOrder(-49m, strategy));
        Assert.False(sut.ShouldGenerateOrder(0m, strategy));
    }

    [Fact]
    public void ShouldGenerateOrder_AmountAboveMinimum_ReturnsTrue()
    {
        var sut = new RebalancingService();
        var strategy = Strategy(50m);

        Assert.True(sut.ShouldGenerateOrder(100m, strategy));
        Assert.True(sut.ShouldGenerateOrder(-100m, strategy));
    }

    [Fact]
    public void ShouldGenerateOrder_ZeroMinimum_ZeroAmount_ReturnsTrue()
    {
        var sut = new RebalancingService();
        var strategy = Strategy(0m);

        Assert.True(sut.ShouldGenerateOrder(0m, strategy));
    }

    [Fact]
    public void ShouldGenerateOrder_NullStrategy_ThrowsArgumentNullException()
    {
        var sut = new RebalancingService();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.ShouldGenerateOrder(100m, null!));

        Assert.Equal("strategy", ex.ParamName);
    }
}
