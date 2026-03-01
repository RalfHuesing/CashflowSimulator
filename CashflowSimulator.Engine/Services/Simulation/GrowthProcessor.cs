using CashflowSimulator.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Prozessor für deterministisches monatliches Wachstum der Portfolio-Assets anhand der erwarteten Rendite (μ) der zugehörigen Economic Factors.
/// </summary>
public sealed class GrowthProcessor : ISimulationProcessor
{
    private readonly ILogger<GrowthProcessor>? _logger;

    public GrowthProcessor(ILogger<GrowthProcessor>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void ProcessMonth(SimulationProjectDto project, SimulationState state, DateOnly currentDate)
    {
        ArgumentNullException.ThrowIfNull(state.Portfolio);
        var assets = state.Portfolio.Assets;
        if (assets.Count == 0)
        {
            state.TotalAssets = state.Cash;
            return;
        }

        var updatedAssets = new List<AssetDto>(assets.Count);
        foreach (var asset in assets)
        {
            if (asset.Tranches is { Count: > 0 })
            {
                var trancheSum = asset.Tranches.Sum(t => t.Quantity);
                if (Math.Abs(trancheSum - asset.CurrentQuantity) > 0.0001m)
                {
                    _logger?.LogWarning(
                        "Asset '{AssetName}': CurrentQuantity ({Qty}) weicht von Tranchen-Summe ({TrancheSum}) ab.",
                        asset.Name, asset.CurrentQuantity, trancheSum);
                }
            }

            var factor = project.EconomicFactors?.FirstOrDefault(f => f.Id == asset.EconomicFactorId);
            if (factor == null)
            {
                _logger?.LogWarning("Kein EconomicFactor mit Id '{FactorId}' für Asset '{AssetName}' gefunden, Kurs unverändert", asset.EconomicFactorId, asset.Name);
                updatedAssets.Add(asset);
                continue;
            }

            // Stetige Verzinsung: monatlicher Faktor = e^(μ/12); Geldwerte bleiben decimal
            var mu = factor.ExpectedReturn;
            var monthlyFactor = (decimal)Math.Exp(mu / 12.0);
            var newPrice = asset.CurrentPrice * monthlyFactor;
            var currentValue = newPrice * asset.CurrentQuantity;

            updatedAssets.Add(asset with
            {
                CurrentPrice = newPrice,
                CurrentValue = currentValue
            });
        }

        state.Portfolio = state.Portfolio with { Assets = updatedAssets };

        var portfolioValue = updatedAssets.Sum(a => a.CurrentValue ?? (a.CurrentPrice * a.CurrentQuantity));
        state.TotalAssets = state.Cash + portfolioValue;
    }
}
