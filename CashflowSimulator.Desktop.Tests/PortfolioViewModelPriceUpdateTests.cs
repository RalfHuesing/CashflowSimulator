using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    private sealed class MockPortfolioService : IPortfolioService
    {
        private readonly decimal _newPrice;

        public MockPortfolioService(decimal newPrice)
        {
            _newPrice = newPrice;
        }

        public Task<(List<AssetDto> UpdatedAssets, int UpdatedCount, int ErrorCount)> UpdatePricesAsync(
            IEnumerable<AssetDto> currentAssets, 
            CancellationToken cancellationToken = default)
        {
            var assets = new List<AssetDto>();
            foreach(var a in currentAssets)
            {
                assets.Add(a with { CurrentPrice = _newPrice, CurrentValue = a.CurrentQuantity * _newPrice });
            }
            return Task.FromResult((assets, assets.Count, 0));
        }
    }

    private static SimulationProjectDto CreateProjectWithAssets(params AssetDto[] assets)
    {
        return new SimulationProjectDto
        {
            Portfolio = new PortfolioDto { Assets = new List<AssetDto>(assets) }
        };
    }

    [Fact]
    public async Task UpdatePricesAsync_CallsServiceAndUpdatesProject()
    {
        var asset = new AssetDto { Id = "a1", CurrentQuantity = 10m, CurrentPrice = 100m };
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateProjectWithAssets(asset));
        
        var service = new MockPortfolioService(123.45m);
        var vm = new PortfolioViewModel(projectService, null!, service);

        await vm.UpdatePricesCommand.ExecuteAsync(null);

        Assert.False(vm.IsUpdatingPrices);
        var updated = projectService.Current!.Portfolio!.Assets[0];
        Assert.Equal(123.45m, updated.CurrentPrice);
        Assert.Equal(1234.50m, updated.CurrentValue);
    }

    [Fact]
    public void UpdatePricesCommand_CanExecute_WhenNotUpdating()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(CreateProjectWithAssets(new AssetDto { Id = "a1" }));
        var vm = new PortfolioViewModel(projectService, null!, new MockPortfolioService(100m));

        Assert.True(vm.UpdatePricesCommand.CanExecute(null));
    }
}
