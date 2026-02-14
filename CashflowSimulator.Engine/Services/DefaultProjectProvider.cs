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
            UiSettings = new UiSettingsDto()
        };
    }
}
