namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Definiert die (Pearson-)Korrelation zwischen zwei ökonomischen Faktoren.
/// Die Engine nutzt diese Werte zur Konstruktion einer Korrelationsmatrix und anschließend
/// zur Cholesky-Zerlegung, um korrelierte Zufallsinnovationen zu erzeugen.
/// </summary>
/// <remarks>
/// Korrelation muss im Intervall [-1.0, 1.0] liegen. Die Gesamtmatrix aller Faktoren
/// (inkl. impliziter 1.0 auf der Diagonalen) muss positiv definit sein, damit die
/// Cholesky-Zerlegung existiert – die Validierung stellt dies sicher.
/// </remarks>
public record CorrelationEntryDto
{
    /// <summary>
    /// ID des ersten Faktors (muss in <see cref="SimulationProjectDto.EconomicFactors"/> existieren).
    /// </summary>
    public string FactorIdA { get; init; } = string.Empty;

    /// <summary>
    /// ID des zweiten Faktors (muss in <see cref="SimulationProjectDto.EconomicFactors"/> existieren).
    /// </summary>
    public string FactorIdB { get; init; } = string.Empty;

    /// <summary>
    /// Pearson-Korrelationskoeffizient zwischen den beiden Faktoren. Gültiger Bereich: -1.0 bis 1.0.
    /// 0 = unkorreliert; 1 = perfekt positiv; -1 = perfekt negativ korreliert.
    /// </summary>
    public double Correlation { get; init; }
}
