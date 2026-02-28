namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Ergebnis eines einzelnen Simulationsmonats (Zustand nach Anwendung der Cashflow-Processors).
/// </summary>
public record MonthlyResultDto
{
    /// <summary>Alter zum Monatsende (z. B. 35.08 für 35 Jahre + 1 Monat).</summary>
    public double Age { get; init; }

    /// <summary>Laufindex des Monats (0 = erster Monat).</summary>
    public int MonthIndex { get; init; }

    /// <summary>Liquidität (Bargeld) am Monatsende.</summary>
    public decimal CashBalance { get; init; }

    /// <summary>Gesamtvermögen am Monatsende. In Slice 1 identisch mit <see cref="CashBalance"/> (kein Depot).</summary>
    public decimal TotalAssets { get; init; }

    /// <summary>Detaillierte Cashflows in diesem Monat (Stream-Name, Typ, Betrag).</summary>
    public List<CashflowSnapshotEntryDto> CashflowSnapshots { get; init; } = [];
}
