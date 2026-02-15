using System.ComponentModel;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Typ des stochastischen Prozesses, mit dem ein ökonomischer Faktor (z. B. Aktienindex, Inflation)
/// über die Zeit modelliert wird.
/// </summary>
/// <remarks>
/// Die Wahl des Modells ist fachlich begründet:
/// <list type="bullet">
///   <item><strong>GeometricBrownianMotion (GBM):</strong> Typisch für Aktien, Indizes (z. B. MSCI World) oder Rohstoffe (Gold).
///   Der Prozess ist nicht mittelwert-revertierend; Renditen werden als log-normal angenommen. Geeignet für Assets,
///   die langfristig „treiben“ können (positiver Drift) und keine natürliche Rückkehr zu einem Niveau haben.</item>
///   <item><strong>OrnsteinUhlenbeck (OU):</strong> Typisch für Zinsen, Inflation oder Mean-Reversion-assoziierte Größen.
///   Der Prozess kehrt zu einem langfristigen Mittelwert (Mean) zurück; die Geschwindigkeit steuert
///   <see cref="EconomicFactorDto.MeanReversionSpeed"/>. Geeignet, wenn die Realität eher „band-begrenzt“ ist
///   (z. B. Inflationsraten schwanken um 2 %, Zinsen um ein Niveau) statt unbegrenzt zu driften.</item>
/// </list>
/// </remarks>
public enum StochasticModelType
{
    /// <summary>
    /// Geometrische Brownsche Bewegung (GBM). Modell: dS = μ·S·dt + σ·S·dW.
    /// Typisch für Aktien/Indizes; keine Mean-Reversion; <see cref="EconomicFactorDto.MeanReversionSpeed"/> wird ignoriert.
    /// </summary>
    [Description("Geometrische Brownsche Bewegung (GBM)")]
    GeometricBrownianMotion,

    /// <summary>
    /// Ornstein-Uhlenbeck-Prozess. Modell: dX = θ·(μ − X)·dt + σ·dW.
    /// Typisch für Zinsen/Inflation; Rückkehr zum langfristigen Mittelwert; <see cref="EconomicFactorDto.MeanReversionSpeed"/> (θ) steuert die Stärke.
    /// </summary>
    [Description("Ornstein-Uhlenbeck (Mean Reversion)")]
    OrnsteinUhlenbeck
}
