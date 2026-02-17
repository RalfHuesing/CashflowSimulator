namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Ein Eintrag in einem Allokationsprofil: Zielgewichtung einer Anlageklasse.
/// Referenziert <see cref="AssetClassDto.Id"/>.
/// </summary>
public record AllocationProfileEntryDto
{
    /// <summary>
    /// ID der Anlageklasse, die in diesem Profil gewichtet wird.
    /// </summary>
    public string AssetClassId { get; init; } = string.Empty;

    /// <summary>
    /// Zielgewichtung (0,0 bis 1,0). 0,6 = 60 %.
    /// </summary>
    public decimal TargetWeight { get; init; }
}
