using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Engine.Services;

/// <summary>
/// Erzeugt ein Standard-Szenario mit Domain-Defaults für Meta und Parameters.
/// </summary>
public sealed class DefaultProjectProvider : IDefaultProjectProvider
{
    public SimulationProjectDto CreateDefault()
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.DateTime);
        var simulationStart = new DateOnly(today.Year, today.Month, 1);

        const int defaultRetirementAge = 67;
        const int defaultLifeExpectancy = 95;
        var birthYear = today.Year - 40;
        var dateOfBirth = new DateOnly(birthYear, 1, 1);
        var retirementDate = new DateOnly(birthYear + defaultRetirementAge, 1, 1);
        var simulationEnd = new DateOnly(birthYear + defaultLifeExpectancy, 1, 1);

        var streams = new List<CashflowStreamDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Gehalt Netto",
                Type = CashflowType.Income,
                Amount = 3200m,
                Interval = "Monthly",
                StartDate = simulationStart,
                EndDate = retirementDate
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wohnen (Miete / Hausgeld)",
                Type = CashflowType.Expense,
                Amount = 1200m,
                Interval = "Monthly",
                StartDate = simulationStart,
                EndDate = null
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Lebenshaltung",
                Type = CashflowType.Expense,
                Amount = 900m,
                Interval = "Monthly",
                StartDate = simulationStart,
                EndDate = null
            }
        };

        var events = new List<CashflowEventDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Geplante Anschaffung",
                Type = CashflowType.Expense,
                Amount = 15000m,
                TargetDate = simulationStart.AddYears(5),
                EarliestMonthOffset = -12,
                LatestMonthOffset = 24
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Einmalige Einnahme",
                Type = CashflowType.Income,
                Amount = 5000m,
                TargetDate = simulationStart.AddYears(2),
                EarliestMonthOffset = null,
                LatestMonthOffset = null
            }
        };

        return new SimulationProjectDto
        {
            Meta = new MetaDto
            {
                ScenarioName = "Standard-Szenario",
                CreatedAt = now
            },
            Parameters = new SimulationParametersDto
            {
                SimulationStart = simulationStart,
                SimulationEnd = simulationEnd,
                DateOfBirth = dateOfBirth,
                RetirementDate = retirementDate,
                InitialLiquidCash = 0,
                CurrencyCode = "EUR"
            },
            Streams = streams,
            Events = events,
            UiSettings = new UiSettingsDto()
        };
    }
}
