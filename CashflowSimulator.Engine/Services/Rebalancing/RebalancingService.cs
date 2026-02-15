using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Rebalancing;

/// <summary>
/// Guard Clause: Keine Order, wenn die Betragsdifferenz unter der Mindest-Transaktionsgröße liegt.
/// </summary>
public sealed class RebalancingService : IRebalancingService
{
    /// <inheritdoc />
    public bool ShouldGenerateOrder(decimal targetMinusCurrent, StrategyProfileDto strategy)
    {
        return Math.Abs(targetMinusCurrent) >= strategy.MinimumTransactionAmount;
    }
}
