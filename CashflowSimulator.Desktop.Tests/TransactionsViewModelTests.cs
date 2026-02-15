using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Features.Portfolio;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class TransactionsViewModelTests
{
    [Fact]
    public void Delete_ByTransactionId_RemovesCorrectTransaction()
    {
        var assetId = Guid.NewGuid().ToString();
        var txId = Guid.NewGuid().ToString();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [],
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = assetId,
                        Name = "ETF A",
                        Isin = "DE000123",
                        AssetType = AssetType.Etf,
                        AssetClassId = "class1",
                        CurrentPrice = 100m,
                        EconomicFactorId = "f1",
                        IsActiveSavingsInstrument = true,
                        TaxType = TaxType.EquityFund,
                        CurrentQuantity = 10,
                        CurrentValue = 1000m,
                        Transactions =
                        [
                            new TransactionDto { Id = txId, Date = DateOnly.FromDateTime(DateTime.Today), Type = TransactionType.Buy, Quantity = 10, PricePerUnit = 100m, TotalAmount = 1000m, TaxAmount = 0 },
                            new TransactionDto { Id = Guid.NewGuid().ToString(), Date = DateOnly.FromDateTime(DateTime.Today).AddDays(-1), Type = TransactionType.Buy, Quantity = 5, PricePerUnit = 100m, TotalAmount = 500m, TaxAmount = 0 }
                        ]
                    }
                ]
            },
            UiSettings = new UiSettingsDto()
        };
        var service = new CurrentProjectService();
        service.SetCurrent(project);

        var vm = new TransactionsViewModel(service, null!);
        var entryToDelete = vm.AllTransactions.First(e => e.Transaction.Id == txId);
        vm.SelectedItem = entryToDelete;
        vm.DeleteCommand.Execute(null);

        var current = service.Current;
        Assert.NotNull(current?.Portfolio);
        var asset = current.Portfolio.Assets.First(a => a.Id == assetId);
        Assert.Single(asset.Transactions);
        Assert.NotEqual(txId, asset.Transactions[0].Id);
    }
}
