using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Eine Anlageklasse (Bucket) für die strategische Zielallokation des Portfolios.
/// Beispiele: "Aktien Welt 70 %", "Schwellenländer 10 %", "Sicherheitsbaustein 20 %".
/// Die Summe der <see cref="TargetWeight"/> aller Klassen sollte idealerweise 1,0 (100 %) ergeben.
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

    /// <summary>
    /// Zielgewichtung der Anlageklasse im Portfolio (0,0 bis 1,0). 0,7 = 70 %.
    /// </summary>
    public double TargetWeight { get; init; }

    /// <summary>
    /// Optionale Farbe für Diagramme/Übersichten (Hex, z. B. "#1E88E5").
    /// </summary>
    public string? Color { get; init; }
}
