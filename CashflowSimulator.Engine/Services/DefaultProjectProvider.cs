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

        // 4. Projekt zusammenbauen
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
            UiSettings = new UiSettingsDto()
        };
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
                Interval = "Monthly",
                StartDate = start,
                EndDate = retirement // Gehalt endet bei Renteneintritt
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Gesetzliche Rente (Prognose Netto)",
                Type = CashflowType.Income,
                Amount = 1950m,
                Interval = "Monthly",
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
                Interval = "Monthly",
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Lebenshaltung (Supermarkt, Drogerie)",
                Type = CashflowType.Expense,
                Amount = 450m,
                Interval = "Monthly",
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Freizeit & Hobby",
                Type = CashflowType.Expense,
                Amount = 200m,
                Interval = "Monthly",
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
                Interval = "Monthly",
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Digitale Abos & Internet",
                Type = CashflowType.Expense,
                Amount = 65m,
                Interval = "Monthly",
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
                Interval = "Yearly",
                StartDate = start,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Urlaubsbudget (Sommer)",
                Type = CashflowType.Expense,
                Amount = 2000m,
                Interval = "Yearly",
                StartDate = new DateOnly(start.Year, 6, 1),
                EndDate = retirement // Im Alter ggf. anderes Reiseverhalten (hier vereinfacht)
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Weihnachten & Geschenke",
                Type = CashflowType.Expense,
                Amount = 600m,
                Interval = "Yearly",
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