namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Wiederkehrender Cashflow (z. B. Gehalt, Miete).
/// </summary>
public record CashflowStreamDto
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public CashflowType Type { get; init; }
    public decimal Amount { get; init; }
    public string Interval { get; init; } = "Monthly";

    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// Optional: ID eines ökonomischen Faktors zur Dynamisierung (z. B. Inflation).
    /// null = nominal (keine Dynamisierung).
    /// </summary>
    public string? EconomicFactorId { get; init; }
}
