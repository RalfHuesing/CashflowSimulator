using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Rebalancing;

/// <summary>
/// Prüft, ob eine Rebalancing-Order ausgeführt werden soll (Mindest-Transaktionsgröße).
/// </summary>
public interface IRebalancingService
{
    /// <summary>
    /// Liefert true, wenn für die gegebene Differenz (Target minus Ist) eine Order generiert werden soll.
    /// Wenn <c>|diff| &lt; strategy.MinimumTransactionAmount</c>, wird false zurückgegeben (keine Mikro-Transaktion).
    /// </summary>
    /// <param name="targetMinusCurrent">Differenz in Währungseinheiten (Sollwert minus Istwert).</param>
    /// <param name="strategy">Strategie-Profil mit MinimumTransactionAmount.</param>
    bool ShouldGenerateOrder(decimal targetMinusCurrent, StrategyProfileDto strategy);
}
