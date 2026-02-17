using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Ein benanntes Soll-Allokationsprofil (z. B. "Rente sicher 60/40").
/// Wird von <see cref="LifecyclePhaseDto"/> referenziert. Die Engine nutzt es
/// für Sparplan-Verteilung und Rebalancing in der jeweiligen Phase.
/// </summary>
public record AllocationProfileDto : IIdentifiable
{
    /// <inheritdoc />
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Anzeigename des Profils (z. B. "Aufbau", "Rente sicher").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Zielgewichtungen pro Anlageklasse. Summe sollte 1,0 (100 %) ergeben.
    /// Referenziert <see cref="AssetClassDto.Id"/>.
    /// </summary>
    public List<AllocationProfileEntryDto> Entries { get; init; } = [];
}
