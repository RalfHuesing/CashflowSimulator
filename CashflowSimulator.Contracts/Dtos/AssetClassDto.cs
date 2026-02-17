using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Eine Anlageklasse (Bucket) für die strategische Zielallokation des Portfolios.
/// Definiert nur die Kategorien (z. B. "Aktien Welt", "Schwellenländer", "Sicherheitsbaustein").
/// Zielgewichtungen werden in <see cref="AllocationProfileDto"/> pro Lebensphase festgelegt.
/// </summary>
public record AssetClassDto : IIdentifiable
{
    /// <summary>
    /// Eindeutige ID der Anlageklasse (z. B. Guid).
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Anzeigename der Anlageklasse (z. B. "Aktien Welt", "Sicherheitsbaustein").
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
