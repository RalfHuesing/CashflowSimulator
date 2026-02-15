using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Geplantes Einzelereignis (z. B. Anschaffung) mit optionaler Toleranz für die Simulation.
/// </summary>
public record CashflowEventDto : IIdentifiable
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public CashflowType Type { get; init; }
    public decimal Amount { get; init; }

    public DateOnly TargetDate { get; init; }
    public int? EarliestMonthOffset { get; init; }
    public int? LatestMonthOffset { get; init; }

    /// <summary>
    /// Optional: ID eines ökonomischen Faktors zur Dynamisierung (z. B. Inflation).
    /// null = nominal (keine Dynamisierung).
    /// </summary>
    public string? EconomicFactorId { get; init; }
}
