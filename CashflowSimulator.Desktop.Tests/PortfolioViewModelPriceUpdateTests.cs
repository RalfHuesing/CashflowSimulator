using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Portfolio;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Integrationstests für die Kursaktualisierungs-Logik im <see cref="PortfolioViewModel"/>.
/// </summary>
public sealed class PortfolioViewModelPriceUpdateTests
{
    /// <summary>Stub: liefert einen festen Kurs für jede Abfrage (deterministisch für Tests).</summary>
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

    private static SimulationProjectDto CreateProjectWithAssets(params AssetDto[] assets)
    {
        return new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto
            {
                SimulationStart = DateOnly.FromDateTime(DateTime.Today),
                SimulationEnd = DateOnly.FromDateTime(DateTime.Today.AddYears(30)),
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-40)),
                InitialLiquidCash = 0,
                CurrencyCode = "EUR"
            },
            UiSettings = new UiSettingsDto(),
            Portfolio = new PortfolioDto { Assets = assets.ToList() }
        };
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

        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateProjectWithAssets(asset1, asset2));
        var stockService = new FixedPriceStockPriceService(fixedPrice);
        var vm = new PortfolioViewModel(projectService, null!, stockService);

        await vm.UpdatePricesCommand.ExecuteAsync(null);

        Assert.False(vm.IsUpdatingPrices);
        var project = projectService.Current;
        Assert.NotNull(project?.Portfolio);
        Assert.Equal(2, project.Portfolio.Assets.Count);

        var updated1 = project.Portfolio.Assets.First(a => a.Id == "a1");
        Assert.Equal(fixedPrice, updated1.CurrentPrice);
        Assert.Equal(10m * fixedPrice, updated1.CurrentValue);

        var updated2 = project.Portfolio.Assets.First(a => a.Id == "a2");
        Assert.Equal(fixedPrice, updated2.CurrentPrice);
        Assert.Equal(20m * fixedPrice, updated2.CurrentValue);

        Assert.Contains("2 Kurse aktualisiert", vm.UpdateStatus);
    }

    [Fact]
    public async Task UpdatePricesAsync_WithNoAssets_SetsStatusMessage()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateProjectWithAssets());
        var stockService = new FixedPriceStockPriceService(100m);
        var vm = new PortfolioViewModel(projectService, null!, stockService);

        await vm.UpdatePricesCommand.ExecuteAsync(null);

        Assert.False(vm.IsUpdatingPrices);
        Assert.Contains("Keine Assets", vm.UpdateStatus);
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

        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateProjectWithAssets(asset));
        var stockService = new FixedPriceStockPriceService(fixedPrice);
        var vm = new PortfolioViewModel(projectService, null!, stockService);

        await vm.UpdatePricesCommand.ExecuteAsync(null);

        var updated = projectService.Current!.Portfolio!.Assets[0];
        Assert.Equal(fixedPrice, updated.CurrentPrice);
        Assert.Equal(5m * fixedPrice, updated.CurrentValue);
        Assert.Contains("1 Kurse aktualisiert", vm.UpdateStatus);
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

        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateProjectWithAssets(asset));
        var stockService = new FixedPriceStockPriceService(0, success: false);
        var vm = new PortfolioViewModel(projectService, null!, stockService);

        await vm.UpdatePricesCommand.ExecuteAsync(null);

        Assert.False(vm.IsUpdatingPrices);
        var updated = projectService.Current!.Portfolio!.Assets[0];
        Assert.Equal(100m, updated.CurrentPrice); // unverändert
        Assert.Contains("1 Fehler", vm.UpdateStatus);
    }

    [Fact]
    public void UpdatePricesCommand_CanExecute_WhenNotUpdating()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateProjectWithAssets(new AssetDto { Id = "a1", Name = "X", Isin = "DE" }));
        var vm = new PortfolioViewModel(projectService, null!, new FixedPriceStockPriceService(100m));

        Assert.True(vm.UpdatePricesCommand.CanExecute(null));
    }
}
