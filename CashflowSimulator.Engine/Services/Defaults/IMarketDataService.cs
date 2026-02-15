using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Liefert die Standard-Marktdaten für ein Default-Szenario:
/// ökonomische Faktoren (Inflation, Aktien, Anleihen) und Korrelationen zwischen ihnen.
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// Liefert die Standard-Marktfaktoren aus deutscher Sicht
    /// (z. B. Inflation VPI, Aktien Welt, Geldmarkt/Anleihen, Schwellenländer).
    /// </summary>
    List<EconomicFactorDto> GetEconomicFactors();

    /// <summary>
    /// Liefert die Standard-Korrelationen zwischen den übergebenen Faktoren
    /// (z. B. Aktien vs. Anleihen leicht negativ, Aktien vs. Schwellenländer positiv).
    /// </summary>
    /// <param name="factors">Die zuvor von <see cref="GetEconomicFactors"/> gelieferten Faktoren.</param>
    List<CorrelationEntryDto> GetCorrelations(List<EconomicFactorDto> factors);
}
