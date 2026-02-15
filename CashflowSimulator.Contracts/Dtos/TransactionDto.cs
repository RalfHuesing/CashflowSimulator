using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Eine einzelne Transaktion (Kauf, Verkauf, Ausschüttung, Vorabpauschale) an einem Asset.
/// Die Historie aller Transaktionen ist die Grundlage für die FIFO-Steuerberechnung bei Verkäufen:
/// Die Engine kann anhand der Kaufdaten und -mengen die zuerst gekauften Anteile zuordnen und
/// daraus Gewinn/Verlust und Steuerlast ermitteln.
/// </summary>
public record TransactionDto : IIdentifiable
{
    /// <inheritdoc />
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Datum der Transaktion (Wertstellung bzw. Handelsdatum, je nach fachlicher Definition).
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Art der Transaktion (Kauf, Verkauf, Ausschüttung, Vorabpauschale).
    /// </summary>
    public TransactionType Type { get; init; }

    /// <summary>
    /// Anzahl der Anteile/Stück (bei Kauf/Verkauf). Bei Dividend/TaxPrepayment kann 0 oder eine
    /// rechnerische Menge sein – fachlich steht hier ggf. die betroffene Stückzahl.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Preis pro Stück zum Transaktionszeitpunkt (z. B. Einstandskurs bei Kauf).
    /// Einheit: Währung (decimal für Geldbeträge).
    /// </summary>
    public decimal PricePerUnit { get; init; }

    /// <summary>
    /// Gesamtbetrag der Transaktion (z. B. Quantity × PricePerUnit bei Kauf/Verkauf, oder
    /// Ausschüttungsbetrag bei Dividend). Einheit: Währung (decimal).
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// Steueranteil dieser Transaktion (z. B. Kapitalertragsteuer auf Ausschüttung oder
    /// auf realisierten Gewinn bei Verkauf). Einheit: Währung (decimal).
    /// </summary>
    public decimal TaxAmount { get; init; }
}
