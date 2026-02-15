using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Services;

/// <summary>
/// Erzeugt ein erweitertes Standard-Szenario für einen durchschnittlichen deutschen Single-Haushalt.
/// Der Code wurde in thematische Methoden unterteilt, um die Wartbarkeit der Datensätze zu verbessern.
/// </summary>
public sealed class DefaultProjectProvider : IDefaultProjectProvider
{
    // Konstanten für das Profil
    private const int CurrentAge = 30;
    private const int RetirementAge = 67;
    private const int LifeExpectancy = 90;
    private const string Currency = "EUR";

    public SimulationProjectDto CreateDefault()
    {
        // 1. Zeitliche Basis berechnen
        var (now, simulationStart, dateOfBirth, retirementDate, simulationEnd) = CalculateDates();

        // 2. Datenströme (Streams) aus verschiedenen Kategorien zusammenführen
        // Hier nutzen wir den Collection-Spread-Operator, um die Listen flach zusammenzufügen.
        List<CashflowStreamDto> streams =
        [
            .. GetIncomeStreams(simulationStart, retirementDate),
            .. GetLivingExpenses(simulationStart),
            .. GetMobilityAndSubscriptions(simulationStart),
            .. GetYearlyExpenses(simulationStart, retirementDate)
        ];

        // 3. Einzelereignisse (Events) abrufen
        List<CashflowEventDto> events = GetLifeEvents(simulationStart, retirementDate);

        // 4. Marktfaktoren (stochastische Faktoren) aus deutscher Sicht
        List<EconomicFactorDto> economicFactors = GetDefaultEconomicFactors();

        // 5. Standard-Korrelationen zwischen Faktoren
        List<CorrelationEntryDto> correlations = GetDefaultCorrelations(economicFactors);

        // 6. Anlageklassen (Zielallokation)
        List<AssetClassDto> assetClasses = GetDefaultAssetClasses();

        // 7. Portfolio mit Vermögenswerten und Beispieldaten
        PortfolioDto portfolio = GetDefaultPortfolio(simulationStart, assetClasses);

        // 8. Projekt zusammenbauen
        return new SimulationProjectDto
        {
            Meta = new MetaDto
            {
                ScenarioName = "Beispiel: Deutscher Single (30 Jahre)",
                CreatedAt = now
            },
            Parameters = new SimulationParametersDto
            {
                SimulationStart = simulationStart,
                SimulationEnd = simulationEnd,
                DateOfBirth = dateOfBirth,
                RetirementDate = retirementDate,
                InitialLiquidCash = 15_000m, // C# Digit Separator für Lesbarkeit
                CurrencyCode = Currency
            },
            Streams = streams,
            Events = events,
            EconomicFactors = economicFactors,
            Correlations = correlations,
            AssetClasses = assetClasses,
            Portfolio = portfolio,
            UiSettings = new UiSettingsDto()
        };
    }

    /// <summary>
    /// Standard-Anlageklassen: Aktien Welt 70 %, Schwellenländer 10 %, Sicherheitsbaustein 20 %.
    /// </summary>
    private static List<AssetClassDto> GetDefaultAssetClasses()
    {
        return
        [
            new AssetClassDto { Id = "Aktien_Welt", Name = "Aktien Welt", TargetWeight = 0.70, Color = "#1E88E5" },
            new AssetClassDto { Id = "Schwellenlaender", Name = "Schwellenländer", TargetWeight = 0.10, Color = "#43A047" },
            new AssetClassDto { Id = "Sicherheitsbaustein", Name = "Sicherheitsbaustein", TargetWeight = 0.20, Color = "#FB8C00" }
        ];
    }

    /// <summary>
    /// Standard-Marktfaktoren aus deutscher Sicht: Inflation (VPI), Aktien Welt (ETF), Geldmarkt/Anleihen.
    /// </summary>
    private static List<EconomicFactorDto> GetDefaultEconomicFactors()
    {
        return
        [
            new EconomicFactorDto
            {
                Id = "Inflation_VPI",
                Name = "Inflation (VPI)",
                ModelType = StochasticModelType.OrnsteinUhlenbeck,
                ExpectedReturn = 0.02,       // Langfristiger Mittelwert ca. 2 %
                Volatility = 0.01,
                MeanReversionSpeed = 0.3,
                InitialValue = 0.02
            },
            new EconomicFactorDto
            {
                Id = "Aktien_Welt",
                Name = "Aktien Welt (ETF)",
                ModelType = StochasticModelType.GeometricBrownianMotion,
                ExpectedReturn = 0.07,       // Drift ca. 7 % p.a.
                Volatility = 0.15,          // Vola ca. 15 %
                MeanReversionSpeed = 0,
                InitialValue = 100
            },
            new EconomicFactorDto
            {
                Id = "Geldmarkt_Anleihen",
                Name = "Geldmarkt / Anleihen",
                ModelType = StochasticModelType.OrnsteinUhlenbeck,
                ExpectedReturn = 0.02,
                Volatility = 0.03,
                MeanReversionSpeed = 0.2,
                InitialValue = 100
            },
            new EconomicFactorDto
            {
                Id = "Schwellenlaender",
                Name = "Schwellenländer (Aktien)",
                ModelType = StochasticModelType.GeometricBrownianMotion,
                ExpectedReturn = 0.065,
                Volatility = 0.18,
                MeanReversionSpeed = 0,
                InitialValue = 100
            }
        ];
    }

    /// <summary>
    /// Standard-Portfolio: 3x MSCI-World-ETFs (ein aktives Sparinstrument, zwei Altbestände),
    /// 1x Schwellenländer-ETF, 1x Anleihen-ETF, 1x Tagesgeld. Mit exemplarischen Transaktionen.
    /// </summary>
    private static PortfolioDto GetDefaultPortfolio(DateOnly simulationStart, List<AssetClassDto> assetClasses)
    {
        const string factorAktienWelt = "Aktien_Welt";
        const string factorSchwellenlaender = "Schwellenlaender";
        const string factorGeldmarkt = "Geldmarkt_Anleihen";
        var classAktienWelt = assetClasses.First(c => c.Id == "Aktien_Welt").Id;
        var classSchwellenlaender = assetClasses.First(c => c.Id == "Schwellenlaender").Id;
        var classSicherheit = assetClasses.First(c => c.Id == "Sicherheitsbaustein").Id;

        // 1. Vanguard FTSE All-World – aktives Sparinstrument
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
            EconomicFactorId = factorAktienWelt,
            IsActiveSavingsInstrument = true,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 80,
            CurrentValue = 80 * vanguardPrice,
            Transactions = vanguardTransactions
        };

        // 2. iShares MSCI World – Altbestand (nicht mehr bespart)
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
            EconomicFactorId = factorAktienWelt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 25,
            CurrentValue = 25 * isharesPrice,
            Transactions = isharesTransactions
        };

        // 3. HSBC MSCI World – Altbestand
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
            EconomicFactorId = factorAktienWelt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 15,
            CurrentValue = 15 * hsbcPrice,
            Transactions = hsbcTransactions
        };

        // 4. Schwellenländer-ETF
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
            EconomicFactorId = factorSchwellenlaender,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.EquityFund,
            CurrentQuantity = 40,
            CurrentValue = 40 * emPrice,
            Transactions = emTransactions
        };

        // 5. Anleihen-ETF
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
            EconomicFactorId = factorGeldmarkt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.BondFund,
            CurrentQuantity = 100,
            CurrentValue = 100 * bondPrice,
            Transactions = bondTransactions
        };

        // 6. Tagesgeld (Geldmarkt-Faktor)
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
            EconomicFactorId = factorGeldmarkt,
            IsActiveSavingsInstrument = false,
            TaxType = TaxType.None,
            CurrentQuantity = 1,
            CurrentValue = cashPrice,
            Transactions = []
        };

        return new PortfolioDto
        {
            Assets =
            [
                vanguard,
                ishares,
                hsbc,
                em,
                bond,
                cash
            ]
        };
    }

    /// <summary>
    /// Plausible Standard-Korrelationen (z. B. Aktien vs. Anleihen leicht negativ).
    /// </summary>
    private static List<CorrelationEntryDto> GetDefaultCorrelations(List<EconomicFactorDto> factors)
    {
        var idToIndex = factors.Select((f, i) => (f.Id, i)).ToDictionary(x => x.Id, x => x.i);
        if (idToIndex.Count < 2)
            return [];

        var list = new List<CorrelationEntryDto>();
        // Inflation vs. Aktien: leicht positiv
        if (idToIndex.TryGetValue("Inflation_VPI", out var iInf) && idToIndex.TryGetValue("Aktien_Welt", out var iAkt))
            list.Add(new CorrelationEntryDto { FactorIdA = factors[iInf].Id, FactorIdB = factors[iAkt].Id, Correlation = 0.2 });
        // Aktien vs. Anleihen: leicht negativ
        if (idToIndex.TryGetValue("Aktien_Welt", out var iA) && idToIndex.TryGetValue("Geldmarkt_Anleihen", out var iB))
            list.Add(new CorrelationEntryDto { FactorIdA = factors[iA].Id, FactorIdB = factors[iB].Id, Correlation = -0.15 });
        // Aktien Welt vs. Schwellenländer: positiv
        if (idToIndex.TryGetValue("Aktien_Welt", out var iAw) && idToIndex.TryGetValue("Schwellenlaender", out var iEm))
            list.Add(new CorrelationEntryDto { FactorIdA = factors[iAw].Id, FactorIdB = factors[iEm].Id, Correlation = 0.75 });
        return list;
    }

    /// <summary>
    /// Zentralisiert die Berechnung aller relevanten Stichtage.
    /// </summary>
    private static (DateTimeOffset Now, DateOnly SimStart, DateOnly Dob, DateOnly RetStart, DateOnly SimEnd) CalculateDates()
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.DateTime);
        var simulationStart = new DateOnly(today.Year, today.Month, 1);

        var birthYear = today.Year - CurrentAge;
        var dateOfBirth = new DateOnly(birthYear, 5, 15);

        var retirementDate = new DateOnly(birthYear + RetirementAge, 6, 1);
        var simulationEnd = new DateOnly(birthYear + LifeExpectancy, 5, 1);

        return (now, simulationStart, dateOfBirth, retirementDate, simulationEnd);
    }

    /// <summary>
    /// Erzeugt die Einnahmen-Seite (Gehalt, Rente).
    /// </summary>
    private static IEnumerable<CashflowStreamDto> GetIncomeStreams(DateOnly start, DateOnly retirement)
    {
        return
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Gehalt (Netto)",
                Type = CashflowType.Income,
                Amount = 2850m,
                Interval = CashflowInterval.Monthly,
                StartDate = start,
                EndDate = retirement // Gehalt endet bei Renteneintritt
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Gesetzliche Rente (Prognose Netto)",
                Type = CashflowType.Income,
                Amount = 1950m,
                Interval = CashflowInterval.Monthly,
                StartDate = retirement, // Rente beginnt bei Renteneintritt
                EndDate = null
            }
        ];
    }

    /// <summary>
    /// Erzeugt die grundlegenden Lebenshaltungskosten (Wohnen, Essen).
    /// </summary>
    private static IEnumerable<CashflowStreamDto> GetLivingExpenses(DateOnly start)
    {
        return
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wohnen (Warmmiete + Strom)",
                Type = CashflowType.Expense,
                Amount = 1050m,
                Interval = CashflowInterval.Monthly,
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Lebenshaltung (Supermarkt, Drogerie)",
                Type = CashflowType.Expense,
                Amount = 450m,
                Interval = CashflowInterval.Monthly,
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Freizeit & Hobby",
                Type = CashflowType.Expense,
                Amount = 200m,
                Interval = CashflowInterval.Monthly,
                StartDate = start,
                EndDate = null
            }
        ];
    }

    /// <summary>
    /// Erzeugt Kosten für Mobilität, Abos und Verträge.
    /// </summary>
    private static IEnumerable<CashflowStreamDto> GetMobilityAndSubscriptions(DateOnly start)
    {
        return
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mobilität (ÖPNV + Carsharing)",
                Type = CashflowType.Expense,
                Amount = 120m,
                Interval = CashflowInterval.Monthly,
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Digitale Abos & Internet",
                Type = CashflowType.Expense,
                Amount = 65m,
                Interval = CashflowInterval.Monthly,
                StartDate = start,
                EndDate = null
            }
        ];
    }

    /// <summary>
    /// Erzeugt jährliche Kostenblöcke (Versicherungen, Urlaub).
    /// </summary>
    private static IEnumerable<CashflowStreamDto> GetYearlyExpenses(DateOnly start, DateOnly retirement)
    {
        return
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Versicherungen (Haftpflicht, Hausrat, BU)",
                Type = CashflowType.Expense,
                Amount = 350m,
                Interval = CashflowInterval.Yearly,
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Urlaubsbudget (Sommer)",
                Type = CashflowType.Expense,
                Amount = 2000m,
                Interval = CashflowInterval.Yearly,
                StartDate = new DateOnly(start.Year, 6, 1),
                EndDate = retirement // Im Alter ggf. anderes Reiseverhalten (hier vereinfacht)
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Weihnachten & Geschenke",
                Type = CashflowType.Expense,
                Amount = 600m,
                Interval = CashflowInterval.Yearly,
                StartDate = new DateOnly(start.Year, 12, 1),
                EndDate = null
            }
        ];
    }

    /// <summary>
    /// Erzeugt einmalige Lebensereignisse.
    /// </summary>
    private static List<CashflowEventDto> GetLifeEvents(DateOnly start, DateOnly retirement)
    {
        return
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Kauf Gebrauchtwagen",
                Type = CashflowType.Expense,
                Amount = 18000m,
                TargetDate = start.AddYears(4),
                EarliestMonthOffset = -6,
                LatestMonthOffset = 12
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Sabbatical / Weltreise",
                Type = CashflowType.Expense,
                Amount = 12000m,
                TargetDate = start.AddYears(8),
                EarliestMonthOffset = 0,
                LatestMonthOffset = 24
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Hochzeit / Große Feier",
                Type = CashflowType.Expense,
                Amount = 8000m,
                TargetDate = start.AddYears(5),
                EarliestMonthOffset = -12,
                LatestMonthOffset = 36
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Möbel-Upgrade (Küche)",
                Type = CashflowType.Expense,
                Amount = 10000m,
                TargetDate = start.AddYears(12),
                EarliestMonthOffset = -6,
                LatestMonthOffset = 6
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Zahnersatz / Gesundheit im Alter",
                Type = CashflowType.Expense,
                Amount = 5000m,
                TargetDate = retirement.AddYears(5),
                EarliestMonthOffset = 0,
                LatestMonthOffset = 60
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Erbe / Schenkung (Erwartet)",
                Type = CashflowType.Income,
                Amount = 25000m,
                TargetDate = start.AddYears(20),
                EarliestMonthOffset = -24,
                LatestMonthOffset = 24
            }
        ];
    }
}