using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Liefert die Standard-Portfolio-Daten: Anlageklassen (Zielallokation) und
/// das initiale Portfolio mit Vermögenswerten und Beispieldaten.
/// </summary>
public interface IPortfolioDefaultService
{
    /// <summary>
    /// Liefert die Standard-Anlageklassen (z. B. Aktien Welt 70 %, Schwellenländer 10 %, Sicherheit 20 %).
    /// </summary>
    List<AssetClassDto> GetAssetClasses();

    /// <summary>
    /// Liefert das Standard-Portfolio mit ETFs und Tagesgeld inkl. exemplarischer Transaktionen.
    /// </summary>
    /// <param name="simulationStart">Simulationsstart für Transaktionsdaten.</param>
    /// <param name="assetClasses">Die von <see cref="GetAssetClasses"/> gelieferten Anlageklassen.</param>
    PortfolioDto GetPortfolio(DateOnly simulationStart, List<AssetClassDto> assetClasses);
}
