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
        var currentYear = now.Year;
        const int defaultRetirementAge = 67;
        var birthYear = currentYear - 40; // Beispiel: 40 Jahre alt
        var retirementYear = birthYear + defaultRetirementAge;

        return new SimulationProjectDto
        {
            Meta = new MetaDto
            {
                ScenarioName = "Standard-Szenario",
                CreatedAt = now
            },
            Parameters = new SimulationParametersDto
            {
                StartYear = currentYear,
                EndYear = retirementYear + 30, // bis ca. 97
                BirthYear = birthYear,
                RetirementYear = retirementYear,
                InitialLiquidCash = 0,
                CurrencyCode = "EUR"
            }
        };
    }
}
