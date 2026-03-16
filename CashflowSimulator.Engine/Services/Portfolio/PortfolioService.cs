using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Engine.Services.Portfolio;

public class PortfolioService : IPortfolioService
{
    private readonly IStockPriceService _stockPriceService;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(IStockPriceService stockPriceService, ILogger<PortfolioService> logger)
    {
        _stockPriceService = stockPriceService;
        _logger = logger;
    }

    public async Task<(List<AssetDto> UpdatedAssets, int UpdatedCount, int ErrorCount)> UpdatePricesAsync(
        IEnumerable<AssetDto> currentAssets, 
        CancellationToken cancellationToken = default)
    {
        var assets = currentAssets.ToList();
        var updatedAssets = new List<AssetDto>(assets.Count);
        var updatedCount = 0;
        var errorCount = 0;

        foreach (var asset in assets)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var symbol = asset.Isin;
            if (string.IsNullOrWhiteSpace(symbol))
            {
                symbol = asset.Name;
            }

            if (string.IsNullOrWhiteSpace(symbol))
            {
                updatedAssets.Add(asset); // Keine Änderung
                continue;
            }

            try
            {
                var result = await _stockPriceService.GetStockPriceAsync(symbol);
                if (result.Success)
                {
                    var newTotalValue = asset.CurrentQuantity * result.Price;
                    var updated = asset with 
                    { 
                        CurrentPrice = result.Price, 
                        CurrentValue = newTotalValue 
                    };
                    updatedAssets.Add(updated);
                    updatedCount++;
                }
                else
                {
                    updatedAssets.Add(asset);
                    errorCount++;
                    _logger.LogWarning("Kurs für Symbol '{Symbol}' konnte nicht abgerufen werden.", symbol);
                }
            }
            catch (Exception ex)
            {
                updatedAssets.Add(asset);
                errorCount++;
                _logger.LogError(ex, "Fehler beim Abruf des Kurses für Symbol '{Symbol}'.", symbol);
            }
        }

        return (updatedAssets, updatedCount, errorCount);
    }
}
