using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Service für portfolio-bezogene Geschäftslogik (z. B. Kurs-Aktualisierungen).
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// Aktualisiert die Kurse der übergebenen Assets asynchron mittels des IStockPriceService.
    /// Gibt die aktualisierte Liste sowie die Anzahl der Erfolge und Fehler zurück.
    /// </summary>
    Task<(List<AssetDto> UpdatedAssets, int UpdatedCount, int ErrorCount)> UpdatePricesAsync(
        IEnumerable<AssetDto> currentAssets, 
        CancellationToken cancellationToken = default);
}
