using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Standard-Cashflows für einen durchschnittlichen deutschen Single-Haushalt:
/// Einnahmen (Gehalt, Rente), Lebenshaltung, Mobilität, Abos, jährliche Posten, Lebensereignisse.
/// </summary>
public sealed class CashflowDefaultService : ICashflowDefaultService
{
    /// <inheritdoc />
    public List<CashflowStreamDto> GetStreams(DateOnly simulationStart, DateOnly retirementDate)
    {
        List<CashflowStreamDto> streams =
        [
            .. GetIncomeStreams(simulationStart, retirementDate),
            .. GetLivingExpenses(simulationStart),
            .. GetMobilityAndSubscriptions(simulationStart),
            .. GetYearlyExpenses(simulationStart, retirementDate)
        ];
        return streams;
    }

    /// <inheritdoc />
    public List<CashflowEventDto> GetEvents(DateOnly simulationStart, DateOnly retirementDate)
    {
        return
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Kauf Gebrauchtwagen",
                Type = CashflowType.Expense,
                Amount = 18000m,
                TargetDate = simulationStart.AddYears(4),
                EarliestMonthOffset = -6,
                LatestMonthOffset = 12
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Sabbatical / Weltreise",
                Type = CashflowType.Expense,
                Amount = 12000m,
                TargetDate = simulationStart.AddYears(8),
                EarliestMonthOffset = 0,
                LatestMonthOffset = 24
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Hochzeit / Große Feier",
                Type = CashflowType.Expense,
                Amount = 8000m,
                TargetDate = simulationStart.AddYears(5),
                EarliestMonthOffset = -12,
                LatestMonthOffset = 36
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Möbel-Upgrade (Küche)",
                Type = CashflowType.Expense,
                Amount = 10000m,
                TargetDate = simulationStart.AddYears(12),
                EarliestMonthOffset = -6,
                LatestMonthOffset = 6
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Zahnersatz / Gesundheit im Alter",
                Type = CashflowType.Expense,
                Amount = 5000m,
                TargetDate = retirementDate.AddYears(5),
                EarliestMonthOffset = 0,
                LatestMonthOffset = 60
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Erbe / Schenkung (Erwartet)",
                Type = CashflowType.Income,
                Amount = 25000m,
                TargetDate = simulationStart.AddYears(20),
                EarliestMonthOffset = -24,
                LatestMonthOffset = 24
            }
        ];
    }

    private IEnumerable<CashflowStreamDto> GetIncomeStreams(DateOnly start, DateOnly retirement)
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
                EndDate = retirement
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Gesetzliche Rente (Prognose Netto)",
                Type = CashflowType.Income,
                Amount = 1950m,
                Interval = CashflowInterval.Monthly,
                StartDate = retirement,
                EndDate = null
            }
        ];
    }

    private IEnumerable<CashflowStreamDto> GetLivingExpenses(DateOnly start)
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

    private IEnumerable<CashflowStreamDto> GetMobilityAndSubscriptions(DateOnly start)
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

    private IEnumerable<CashflowStreamDto> GetYearlyExpenses(DateOnly start, DateOnly retirement)
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
                EndDate = retirement
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
}
