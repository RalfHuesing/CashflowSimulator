using CashflowSimulator.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Prozessor für Liquiditätssteuerung: investiert Überschuss (state.Cash > 0) anteilig nach Soll-Allokation
/// und deckt Unterdeckung (state.Cash &lt; 0) durch pro-rata Verkauf von Assets.
/// Läuft nach dem CashflowProcessor und vor dem GrowthProcessor.
/// </summary>
public sealed class LiquidityProcessor : ISimulationProcessor
{
    private readonly ILogger<LiquidityProcessor>? _logger;

    public LiquidityProcessor(ILogger<LiquidityProcessor>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void ProcessMonth(SimulationProjectDto project, SimulationState state, DateOnly currentDate)
    {
        ArgumentNullException.ThrowIfNull(state.Portfolio);

        if (state.Cash > 0)
            InvestSurplus(project, state, currentDate);
        else if (state.Cash < 0)
            SellToCoverShortfall(state, currentDate);

        UpdateTotalAssets(state);
    }

    private void InvestSurplus(SimulationProjectDto project, SimulationState state, DateOnly currentDate)
    {
        var entries = LifecyclePhaseResolver.GetAllocationProfileEntries(project, currentDate);
        if (entries == null || entries.Count == 0)
        {
            _logger?.LogDebug("Kein Allokationsprofil für {Date}, Überschuss wird nicht investiert", currentDate);
            return;
        }

        var totalWeight = entries.Sum(e => e.TargetWeight);
        if (totalWeight <= 0)
            return;

        var assets = state.Portfolio.Assets;
        if (assets.Count == 0)
            return;

        var cashToInvest = state.Cash;
        var totalSpent = 0m;
        var updates = new Dictionary<int, AssetDto>();

        foreach (var entry in entries.Where(e => e.TargetWeight > 0))
        {
            var investAmount = cashToInvest * (entry.TargetWeight / totalWeight);
            if (investAmount <= 0)
                continue;

            var candidate = assets
                .Where(a => a.AssetClassId == entry.AssetClassId && a.CurrentPrice > 0)
                .OrderByDescending(a => a.IsActiveSavingsInstrument)
                .ThenBy(a => a.Id)
                .FirstOrDefault();

            if (candidate == null)
                continue;

            var quantity = Math.Floor(investAmount / candidate.CurrentPrice);
            if (quantity <= 0)
                continue;

            var actualSpend = quantity * candidate.CurrentPrice;
            totalSpent += actualSpend;

            var newTransaction = new TransactionDto
            {
                Date = currentDate,
                Type = TransactionType.Buy,
                Quantity = quantity,
                PricePerUnit = candidate.CurrentPrice,
                TotalAmount = actualSpend,
                TaxAmount = 0m
            };

            var existingAsset = updates.TryGetValue(assets.IndexOf(candidate), out var prev) ? prev : candidate;
            var newQuantity = existingAsset.CurrentQuantity + quantity;
            var newValue = existingAsset.CurrentPrice * newQuantity;
            var newTransactions = existingAsset.Transactions.Append(newTransaction).ToList();

            var updatedAsset = existingAsset with
            {
                CurrentQuantity = newQuantity,
                CurrentValue = newValue,
                Transactions = newTransactions
            };

            updates[assets.IndexOf(candidate)] = updatedAsset;
        }

        if (updates.Count == 0)
            return;

        var updatedAssets = assets.Select((a, i) => updates.TryGetValue(i, out var u) ? u : a).ToList();
        state.Cash -= totalSpent;
        state.Portfolio = state.Portfolio with { Assets = updatedAssets };
    }

    private void SellToCoverShortfall(SimulationState state, DateOnly currentDate)
    {
        var assets = state.Portfolio.Assets;
        var deficit = -state.Cash;

        var assetsWithValue = assets
            .Select(a => (Asset: a, Value: a.CurrentValue ?? a.CurrentPrice * a.CurrentQuantity))
            .Where(x => x.Value > 0 && x.Asset.CurrentQuantity > 0 && x.Asset.CurrentPrice > 0)
            .ToList();

        if (assetsWithValue.Count == 0)
        {
            _logger?.LogWarning("Unterdeckung {Deficit:C}, aber kein verkaufbares Asset", deficit);
            return;
        }

        var totalValue = assetsWithValue.Sum(x => x.Value);
        if (totalValue <= 0)
            return;

        var updates = new Dictionary<int, AssetDto>();
        var totalProceeds = 0m;

        foreach (var (Asset, Value) in assetsWithValue)
        {
            var targetSellAmount = deficit * (Value / totalValue);
            var quantityToSell = Math.Min(
                Asset.CurrentQuantity,
                Math.Floor(targetSellAmount / Asset.CurrentPrice));
            if (quantityToSell <= 0)
                continue;

            var actualSellAmount = quantityToSell * Asset.CurrentPrice;
            totalProceeds += actualSellAmount;

            var newTransaction = new TransactionDto
            {
                Date = currentDate,
                Type = TransactionType.Sell,
                Quantity = quantityToSell,
                PricePerUnit = Asset.CurrentPrice,
                TotalAmount = actualSellAmount,
                TaxAmount = 0m
            };

            var newQuantity = Asset.CurrentQuantity - quantityToSell;
            var newValue = Asset.CurrentPrice * newQuantity;
            var newTransactions = Asset.Transactions.Append(newTransaction).ToList();

            var updatedAsset = Asset with
            {
                CurrentQuantity = newQuantity,
                CurrentValue = newValue,
                Transactions = newTransactions
            };

            updates[assets.IndexOf(Asset)] = updatedAsset;
        }

        if (updates.Count == 0)
            return;

        state.Cash += totalProceeds;
        var updatedAssets = assets.Select((a, i) => updates.TryGetValue(i, out var u) ? u : a).ToList();
        state.Portfolio = state.Portfolio with { Assets = updatedAssets };
    }

    private static void UpdateTotalAssets(SimulationState state)
    {
        var portfolioValue = state.Portfolio.Assets.Sum(a => a.CurrentValue ?? a.CurrentPrice * a.CurrentQuantity);
        state.TotalAssets = state.Cash + portfolioValue;
    }
}
