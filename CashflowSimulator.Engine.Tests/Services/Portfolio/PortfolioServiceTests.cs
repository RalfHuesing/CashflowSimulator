using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Engine.Services.Portfolio;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CashflowSimulator.Engine.Tests.Services.Portfolio;

public class PortfolioServiceTests
{
    private sealed class FixedPriceStockPriceService : IStockPriceService
    {
        private readonly decimal _price;
        private readonly bool _success;

        public FixedPriceStockPriceService(decimal price, bool success = true)
        {
            _price = price;
            _success = success;
        }

        public Task<StockPriceResultDto> GetStockPriceAsync(string symbol)
        {
            return Task.FromResult(_success
                ? StockPriceResultDto.SuccessResult(symbol, _price, DateTime.UtcNow)
                : StockPriceResultDto.FailureResult(symbol, "Simulierter Fehler"));
        }
    }

    [Fact]
    public async Task UpdatePricesAsync_WithTwoAssets_UpdatesCurrentPriceAndCurrentValue()
    {
        const decimal fixedPrice = 123.45m;
        var asset1 = new AssetDto
        {
            Id = "a1",
            Name = "ETF A",
            Isin = "DE000123",
            CurrentPrice = 100m,
            CurrentQuantity = 10m,
            CurrentValue = 1000m
        };
        var asset2 = new AssetDto
        {
            Id = "a2",
            Name = "ETF B",
            Isin = "DE000456",
            CurrentPrice = 50m,
            CurrentQuantity = 20m,
            CurrentValue = 1000m
        };

        var stockService = new FixedPriceStockPriceService(fixedPrice);
        var service = new PortfolioService(stockService, NullLogger<PortfolioService>.Instance);

        var assets = new[] { asset1, asset2 };
        var (updatedAssets, updatedCount, errorCount) = await service.UpdatePricesAsync(assets);

        Assert.Equal(2, updatedAssets.Count);
        Assert.Equal(2, updatedCount);
        Assert.Equal(0, errorCount);

        var updated1 = updatedAssets.First(a => a.Id == "a1");
        Assert.Equal(fixedPrice, updated1.CurrentPrice);
        Assert.Equal(10m * fixedPrice, updated1.CurrentValue);

        var updated2 = updatedAssets.First(a => a.Id == "a2");
        Assert.Equal(fixedPrice, updated2.CurrentPrice);
        Assert.Equal(20m * fixedPrice, updated2.CurrentValue);
    }

    [Fact]
    public async Task UpdatePricesAsync_AssetWithoutIsin_UsesNameAsSymbol()
    {
        const decimal fixedPrice = 99.99m;
        var asset = new AssetDto
        {
            Id = "a1",
            Name = "MyETF",
            Isin = "",  // leer → Fallback auf Name
            CurrentPrice = 80m,
            CurrentQuantity = 5m
        };

        var stockService = new FixedPriceStockPriceService(fixedPrice);
        var service = new PortfolioService(stockService, NullLogger<PortfolioService>.Instance);

        var (updatedAssets, updatedCount, errorCount) = await service.UpdatePricesAsync(new[] { asset });

        Assert.Single(updatedAssets);
        Assert.Equal(1, updatedCount);
        Assert.Equal(0, errorCount);

        var updated = updatedAssets[0];
        Assert.Equal(fixedPrice, updated.CurrentPrice);
        Assert.Equal(5m * fixedPrice, updated.CurrentValue);
    }

    [Fact]
    public async Task UpdatePricesAsync_WhenServiceFails_IncrementsErrorCountAndKeepsPrice()
    {
        var asset = new AssetDto
        {
            Id = "a1",
            Name = "ETF",
            Isin = "DE000X",
            CurrentPrice = 100m,
            CurrentQuantity = 1m
        };

        var stockService = new FixedPriceStockPriceService(0, success: false);
        var service = new PortfolioService(stockService, NullLogger<PortfolioService>.Instance);

        var (updatedAssets, updatedCount, errorCount) = await service.UpdatePricesAsync(new[] { asset });

        Assert.Single(updatedAssets);
        Assert.Equal(0, updatedCount);
        Assert.Equal(1, errorCount);

        var updated = updatedAssets[0];
        Assert.Equal(100m, updated.CurrentPrice); // unverändert
    }
}
