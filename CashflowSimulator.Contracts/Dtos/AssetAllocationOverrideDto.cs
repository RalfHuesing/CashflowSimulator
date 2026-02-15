namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Überschreibt die Zielgewichtung einer Anlageklasse innerhalb einer Lebensphase
/// (z. B. weniger Aktien im Alter). Referenziert <see cref="AssetClassDto.Id"/>.
/// </summary>
public record AssetAllocationOverrideDto
{
    /// <summary>
    /// ID der Anlageklasse, deren <see cref="AssetClassDto.TargetWeight"/> überschrieben wird.
    /// </summary>
    public string AssetClassId { get; init; } = string.Empty;

    /// <summary>
    /// Zielgewichtung in dieser Phase (0,0 bis 1,0). 0,3 = 30 %.
    /// </summary>
    public decimal TargetWeight { get; init; }
}
