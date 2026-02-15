namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Strategie-Profil für eine Lebensphase (z. B. Aufbau vs. Entnahme).
/// Wird von <see cref="LifecyclePhaseDto"/> referenziert.
/// </summary>
public record StrategyProfileDto
{
    /// <summary>
    /// Eindeutige ID des Profils (Guid-String).
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Anzeigename des Profils (z. B. "Aufbau", "Entnahme").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Anzahl Monatsausgaben als liquide Reserve (Puffer).
    /// </summary>
    public int CashReserveMonths { get; init; }

    /// <summary>
    /// Abweichungsschwelle für Rebalancing (z. B. 0,05 = 5 %), ab der umgeschichtet wird.
    /// </summary>
    public decimal RebalancingThreshold { get; init; }

    /// <summary>
    /// Wie viele Monate voraus auf geplante Events gespart wird (Liquiditätsplanung).
    /// </summary>
    public int LookaheadMonths { get; init; }
}
