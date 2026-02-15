namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Container für die Gesamtheit der Vermögenswerte (Assets) des Nutzers.
/// Enthält die Liste aller gehaltenen Assets sowie einen Platzhalter für spätere
/// Strategie- bzw. Rebalancing-Regeln (z. B. Zielallokation, Trigger für Umschichtung).
/// </summary>
public record PortfolioDto
{
    /// <summary>
    /// Alle vom Nutzer gehaltenen Assets (ETFs, Anleihen, Bargeld etc.). Jedes Asset
    /// referenziert einen <see cref="EconomicFactorDto"/> für die Wertentwicklung;
    /// mehrere Assets können denselben Faktor nutzen (z. B. verschiedene MSCI-World-ETFs).
    /// </summary>
    public List<AssetDto> Assets { get; init; } = [];

    /// <summary>
    /// Platzhalter für Strategie-/Rebalancing-Konfiguration (Zielgewichte, Regeln, Trigger).
    /// Detaillierung und Struktur folgen in einer späteren Erweiterung.
    /// </summary>
    public string? Strategy { get; init; }
}
