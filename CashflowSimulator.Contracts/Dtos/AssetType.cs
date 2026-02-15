using System.ComponentModel;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Art des Vermögenswerts (Asset) – z. B. ETF, Anleihe, Bargeld.
/// Wird für Anzeige und ggf. steuerliche/regulatorische Zuordnung genutzt.
/// </summary>
public enum AssetType
{
    [Description("ETF")]
    Etf,

    [Description("Anleihe")]
    Bond,

    [Description("Bargeld / Geldmarkt")]
    Cash,

    [Description("Kryptowährung")]
    Crypto,

    [Description("Immobilie")]
    RealEstate
}
