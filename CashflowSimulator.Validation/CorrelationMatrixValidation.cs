using CashflowSimulator.Contracts.Dtos;
using MathNet.Numerics.LinearAlgebra;

namespace CashflowSimulator.Validation;

/// <summary>
/// Prüft, ob die aus <see cref="SimulationProjectDto.EconomicFactors"/> und
/// <see cref="SimulationProjectDto.Correlations"/> gebildete Korrelationsmatrix positiv definit ist
/// (Cholesky-Zerlegbarkeit). Die Engine benötigt dies für korrelierte Zufallsinnovationen.
/// Nutzt MathNet.Numerics für numerisch stabile Prüfung (Cholesky-Zerlegung).
/// </summary>
public static class CorrelationMatrixValidation
{
    private const string NotPositiveDefiniteMessage =
        "Die Korrelationsmatrix ist nicht positiv definit. Bitte Korrelationen so anpassen, dass die Matrix mathematisch zulässig ist (z. B. keine widersprüchlichen Werte).";

    /// <summary>
    /// Baut die Korrelationsmatrix aus Faktoren und Korrelationen und prüft positive Definitheit.
    /// </summary>
    /// <returns>null wenn die Matrix positiv definit ist; sonst eine Fehlermeldung.</returns>
    public static string? GetPositiveDefinitenessError(SimulationProjectDto project)
    {
        var factors = project.EconomicFactors;
        var correlations = project.Correlations;

        if (factors is null || factors.Count == 0)
            return null; // Keine Matrix nötig

        if (factors.Count == 1)
            return null; // 1x1 Matrix [1] ist positiv definit

        // Konsistente Reihenfolge der Faktoren (nach Id)
        var ordered = factors.OrderBy(f => f.Id, StringComparer.Ordinal).ToList();
        var idToIndex = ordered.Select((f, i) => (f.Id, i)).ToDictionary(x => x.Id, x => x.i);

        int n = ordered.Count;
        var R = new double[n, n];
        for (int i = 0; i < n; i++)
            R[i, i] = 1.0;

        foreach (var entry in correlations ?? [])
        {
            if (!idToIndex.TryGetValue(entry.FactorIdA, out int ia) || !idToIndex.TryGetValue(entry.FactorIdB, out int ib))
                continue; // Eintrag verweist auf unbekannten Faktor – ggf. anderer Validator
            if (ia == ib)
                continue;
            double c = Math.Clamp(entry.Correlation, -1.0, 1.0);
            R[ia, ib] = c;
            R[ib, ia] = c;
        }

        return IsPositiveDefinite(R, n) ? null : NotPositiveDefiniteMessage;
    }

    /// <summary>
    /// Prüft positive Definitheit via Cholesky-Zerlegung (MathNet.Numerics).
    /// Keine Exception nach außen: bei nicht positiv definiter Matrix wird false zurückgegeben.
    /// </summary>
    private static bool IsPositiveDefinite(double[,] matrixData, int n)
    {
        Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matrixData);

        try
        {
            _ = matrix.Cholesky();
            return true;
        }
        catch (ArgumentException)
        {
            // MathNet wirft bei nicht positiv definiter Matrix (nicht-positive Diagonale in Cholesky).
            return false;
        }
        catch (ArithmeticException)
        {
            // MathNet kann MatrixNotPositiveDefiniteException (von ArithmeticException) werfen.
            return false;
        }
    }
}
