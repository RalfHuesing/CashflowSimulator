namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Eine Kauf-Tranche: ein Block gehaltener Anteile mit einheitlichem Anschaffungsdatum und -preis.
/// Wird für die FIFO-Steuerberechnung bei Verkäufen benötigt (deutsches Steuerrecht).
/// </summary>
public record AssetTrancheDto
{
    /// <summary>
    /// Kaufdatum (Wertstellung bzw. Handelsdatum).
    /// </summary>
    public DateOnly PurchaseDate { get; init; }

    /// <summary>
    /// Anzahl der Anteile in dieser Tranche.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Anschaffungspreis pro Stück zum Kaufzeitpunkt.
    /// </summary>
    public decimal AcquisitionPricePerUnit { get; init; }
}
