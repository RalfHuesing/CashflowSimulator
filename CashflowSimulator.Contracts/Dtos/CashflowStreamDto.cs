using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Wiederkehrender Cashflow (z. B. Gehalt, Miete).
/// </summary>
public record CashflowStreamDto : IIdentifiable
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public CashflowType Type { get; init; }
    public decimal Amount { get; init; }
    public CashflowInterval Interval { get; init; } = CashflowInterval.Monthly;

    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// Optional: Alter (Jahre), ab dem der Stream aktiv ist. null = keine Altersuntergrenze (StartDate maßgeblich).
    /// </summary>
    public int? StartAge { get; init; }

    /// <summary>
    /// Optional: Alter (Jahre), bis zu dem der Stream aktiv ist. null = keine Altersobergrenze (EndDate maßgeblich).
    /// </summary>
    public int? EndAge { get; init; }

    /// <summary>
    /// Optional: ID eines ökonomischen Faktors zur Dynamisierung (z. B. Inflation).
    /// null = nominal (keine Dynamisierung).
    /// </summary>
    public string? EconomicFactorId { get; init; }
}
