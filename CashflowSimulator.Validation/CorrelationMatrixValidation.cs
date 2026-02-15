using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Validation;

/// <summary>
/// Prüft, ob die aus <see cref="SimulationProjectDto.EconomicFactors"/> und
/// <see cref="SimulationProjectDto.Correlations"/> gebildete Korrelationsmatrix positiv definit ist
/// (Cholesky-Zerlegbarkeit). Die Engine benötigt dies für korrelierte Zufallsinnovationen.
/// </summary>
public static class CorrelationMatrixValidation
{
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

        var Rcopy = (double[,])R.Clone();
        return IsPositiveDefinite(Rcopy, n) ? null : "Die Korrelationsmatrix ist nicht positiv definit. Bitte Korrelationen so anpassen, dass die Matrix mathematisch zulässig ist (z. B. keine widersprüchlichen Werte).";
    }

    /// <summary>
    /// Prüft positive Definitheit via Cholesky-Zerlegung (L*L^T = R).
    /// Bei nicht positiv definiter Matrix liefert Cholesky eine nicht-positive Diagonale.
    /// </summary>
    private static bool IsPositiveDefinite(double[,] A, int n)
    {
        try
        {
            CholeskyDecompose(A, n);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// In-place Cholesky-Zerlegung (untere Dreiecksmatrix L in R überschreiben, obere ignoriert).
    /// Wirft ArgumentException, wenn die Matrix nicht positiv definit ist.
    /// </summary>
    private static void CholeskyDecompose(double[,] A, int n)
    {
        for (int j = 0; j < n; j++)
        {
            double sum = 0;
            for (int k = 0; k < j; k++)
                sum += A[j, k] * A[j, k];
            double d = A[j, j] - sum;
            if (d <= 0)
                throw new ArgumentException("Matrix is not positive definite.");
            A[j, j] = Math.Sqrt(d);
            for (int i = j + 1; i < n; i++)
            {
                sum = 0;
                for (int k = 0; k < j; k++)
                    sum += A[i, k] * A[j, k];
                A[i, j] = (A[i, j] - sum) / A[j, j];
            }
        }
    }
}
