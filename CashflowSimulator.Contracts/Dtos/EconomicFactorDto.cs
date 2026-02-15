using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Definition eines ökonomischen Faktors (Stochastic Factor), der in der Simulation als Zufallsprozess
/// getrieben wird (z. B. Aktienindex, Inflation, Gold). Die Engine nutzt diese Parameter zur Pfad-Generierung
/// (z. B. GBM oder Ornstein-Uhlenbeck) und optional zur Korrelation mehrerer Faktoren (Cholesky).
/// </summary>
/// <remarks>
/// Statistische Größen (Drift, Volatilität, Mean-Reversion) sind als <see cref="double"/> definiert,
/// da sie in stochastischen Formeln und Matrixoperationen verwendet werden. Geldbeträge bleiben in anderen DTOs als <c>decimal</c>.
/// </remarks>
public record EconomicFactorDto : IIdentifiable
{
    /// <summary>
    /// Eindeutige ID des Faktors (z. B. "MSCI_World", "Inflation"). Wird in <see cref="CorrelationEntryDto"/>
    /// referenziert und für deterministisches Seed-Replay verwendet.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Anzeigename des Faktors (z. B. "MSCI World", "Inflationsrate").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Welches stochastische Modell für die Pfad-Generierung verwendet wird
    /// (z. B. GBM für Assets, Ornstein-Uhlenbeck für Zinsen/Inflation).
    /// </summary>
    public StochasticModelType ModelType { get; init; }

    /// <summary>
    /// Erwartete (annualisierte) Rendite bzw. Drift μ (z. B. 0.05 = 5 % p.a.).
    /// Bei GBM: Drift der log-Renditen; bei OU: langfristiger Mittelwert, zu dem der Prozess reverts.
    /// Einheit: dimensionslos (z. B. 0.02 = 2 %).
    /// </summary>
    public double ExpectedReturn { get; init; }

    /// <summary>
    /// Annualisierte Volatilität σ (Standardabweichung der Renditen). Typische Werte z. B. 0.15–0.20 für Aktien.
    /// Einheit: dimensionslos (z. B. 0.18 = 18 %).
    /// </summary>
    public double Volatility { get; init; }

    /// <summary>
    /// Mean-Reversion-Speed θ (nur relevant bei <see cref="StochasticModelType.OrnsteinUhlenbeck"/>).
    /// Steuert, wie schnell der Prozess zum langfristigen Mittelwert <see cref="ExpectedReturn"/> zurückkehrt.
    /// 0 = kein Mean-Reversion-Effekt (annähernd Random Walk); größere Werte = schnellere Rückkehr.
    /// Bei <see cref="StochasticModelType.GeometricBrownianMotion"/> wird dieser Wert von der Engine ignoriert.
    /// </summary>
    public double MeanReversionSpeed { get; init; }

    /// <summary>
    /// Startwert des Faktors zum Simulationsstart (z. B. Indexstand 100, Inflationsrate 0.02).
    /// Einheit abhängig vom Faktor (Index-Punkte, Rate als Dezimal 0.02 = 2 %).
    /// </summary>
    public double InitialValue { get; init; }
}
