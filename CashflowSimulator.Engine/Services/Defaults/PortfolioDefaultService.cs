using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Standard-Anlageklassen und Portfolio: 3x MSCI World, Schwellenländer, Anleihen, Tagesgeld
/// mit exemplarischen Transaktionen.
/// </summary>
public sealed class PortfolioDefaultService : IPortfolioDefaultService
{
    private const string FactorAktienWelt = "Aktien_Welt";
    private const string FactorSchwellenlaender = "Schwellenlaender";
    private const string FactorGeldmarkt = "Geldmarkt_Anleihen";

    /// <inheritdoc />
    public List<AssetClassDto> GetAssetClasses()
    {
        return
        [
            new AssetClassDto { Id = "Aktien_Welt", Name = "Aktien Welt", TargetWeight = 0.70, Color = "#1E88E5" },
            new AssetClassDto { Id = "Schwellenlaender", Name = "Schwellenländer", TargetWeight = 0.10, Color = "#43A047" },
            new AssetClassDto { Id = "Sicherheitsbaustein", Name = "Sicherheitsbaustein", TargetWeight = 0.20, Color = "#FB8C00" }
        ];
    }

    /// <inheritdoc />
    public PortfolioDto GetPortfolio(DateOnly simulationStart, List<AssetClassDto> assetClasses)
    {
        var classAktienWelt = assetClasses.First(c => c.Id == "Aktien_Welt").Id;
        var classSchwellenlaender = assetClasses.First(c => c.Id == "Schwellenlaender").Id;
        var classSicherheit = assetClasses.First(c => c.Id == "Sicherheitsbaustein").Id;

        var vanguardId = Guid.NewGuid().ToString();
        const decimal vanguardPrice = 105m;
        var vanguardTransactions = new List<TransactionDto>
        {
            new() { Date = simulationStart.AddMonths(-24), Type = TransactionType.Buy, Quantity = 50, PricePerUnit = 98.50m, TotalAmount = 4925m, TaxAmount = 0 },
            new() { Date = simulationStart.AddMonths(-12), Type = TransactionType.Buy, Quantity = 30, PricePerUnit = 102.20m, TotalAmount = 3066m, TaxAmount = 0 }
        };
        var vanguard = new AssetDto
        {
            Id = vanguardId,
            Name = "Vanguard FTSE All-World UCITS ETF",
            Isin = "IE00B3RBWM25",
            AssetType = AssetType.Etf,
            AssetClassId = classAktienWelt,
            CurrentPrice = vanguardPrice,
            EconomicFactorId = FactorAktienWelt,
            IsActiveSavingsInstrument = true,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 80,
            CurrentValue = 80 * vanguardPrice,
            Transactions = vanguardTransactions
        };

        var isharesId = Guid.NewGuid().ToString();
        const decimal isharesPrice = 105m;
        var isharesTransactions = new List<TransactionDto>
        {
            new() { Date = simulationStart.AddMonths(-60), Type = TransactionType.Buy, Quantity = 25, PricePerUnit = 72.00m, TotalAmount = 1800m, TaxAmount = 0 }
        };
        var ishares = new AssetDto
        {
            Id = isharesId,
            Name = "iShares MSCI World UCITS ETF",
            Isin = "IE00B0M62Q72",
            AssetType = AssetType.Etf,
            AssetClassId = classAktienWelt,
            CurrentPrice = isharesPrice,
            EconomicFactorId = FactorAktienWelt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 25,
            CurrentValue = 25 * isharesPrice,
            Transactions = isharesTransactions
        };

        var hsbcId = Guid.NewGuid().ToString();
        const decimal hsbcPrice = 105m;
        var hsbcTransactions = new List<TransactionDto>
        {
            new() { Date = simulationStart.AddMonths(-36), Type = TransactionType.Buy, Quantity = 15, PricePerUnit = 85.00m, TotalAmount = 1275m, TaxAmount = 0 }
        };
        var hsbc = new AssetDto
        {
            Id = hsbcId,
            Name = "HSBC MSCI World UCITS ETF",
            Isin = "IE00B4X9L533",
            AssetType = AssetType.Etf,
            AssetClassId = classAktienWelt,
            CurrentPrice = hsbcPrice,
            EconomicFactorId = FactorAktienWelt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 15,
            CurrentValue = 15 * hsbcPrice,
            Transactions = hsbcTransactions
        };

        var emId = Guid.NewGuid().ToString();
        const decimal emPrice = 24m;
        var emTransactions = new List<TransactionDto>
        {
            new() { Date = simulationStart.AddMonths(-18), Type = TransactionType.Buy, Quantity = 40, PricePerUnit = 22.50m, TotalAmount = 900m, TaxAmount = 0 }
        };
        var em = new AssetDto
        {
            Id = emId,
            Name = "iShares Core MSCI EM IMI UCITS ETF",
            Isin = "IE00BKM4GZ66",
            AssetType = AssetType.Etf,
            AssetClassId = classSchwellenlaender,
            CurrentPrice = emPrice,
            EconomicFactorId = FactorSchwellenlaender,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 40,
            CurrentValue = 40 * emPrice,
            Transactions = emTransactions
        };

        var bondId = Guid.NewGuid().ToString();
        const decimal bondPrice = 99m;
        var bondTransactions = new List<TransactionDto>
        {
            new() { Date = simulationStart.AddMonths(-12), Type = TransactionType.Buy, Quantity = 100, PricePerUnit = 98.00m, TotalAmount = 9800m, TaxAmount = 0 }
        };
        var bond = new AssetDto
        {
            Id = bondId,
            Name = "iShares Core Global Aggregate Bond UCITS ETF",
            Isin = "IE00BDBRDM35",
            AssetType = AssetType.Etf,
            AssetClassId = classSicherheit,
            CurrentPrice = bondPrice,
            EconomicFactorId = FactorGeldmarkt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.BondFund,
            CurrentQuantity = 100,
            CurrentValue = 100 * bondPrice,
            Transactions = bondTransactions
        };

        var cashId = Guid.NewGuid().ToString();
        const decimal cashPrice = 15000m;
        var cash = new AssetDto
        {
            Id = cashId,
            Name = "Tagesgeld (Notgroschen)",
            Isin = "",
            AssetType = AssetType.Cash,
            AssetClassId = classSicherheit,
            CurrentPrice = cashPrice,
            EconomicFactorId = FactorGeldmarkt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.None,
            CurrentQuantity = 1,
            CurrentValue = cashPrice,
            Transactions = []
        };

        return new PortfolioDto
        {
            Assets = [vanguard, ishares, hsbc, em, bond, cash]
        };
    }
}
