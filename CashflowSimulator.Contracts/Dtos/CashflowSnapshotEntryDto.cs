namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Einzelner Eintrag in den Cashflow-Snapshots eines Monats (Stream-Name, Typ, Betrag).
/// </summary>
public record CashflowSnapshotEntryDto
{
    /// <summary>Anzeigename des Streams (z. B. "Gehalt", "Miete").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Einnahme oder Ausgabe.</summary>
    public CashflowType CashflowType { get; init; }

    /// <summary>Betrag in diesem Monat (positiv; Richtung ergibt sich aus <see cref="CashflowType"/>).</summary>
    public decimal Amount { get; init; }
}
