using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Standard-Implementierung: Berechnet Stichtage und Parameter für einen durchschnittlichen
/// deutschen Single-Haushalt (Alter 30, Rente 67, Lebenserwartung 90).
/// </summary>
public sealed class SimulationTimeService : ISimulationTimeService
{
    private const int CurrentAge = 30;
    private const int RetirementAge = 67;
    private const int LifeExpectancy = 90;
    private const string Currency = "EUR";

    /// <inheritdoc />
    public DefaultTimeContext GetDefaultTimeContext()
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.DateTime);
        var simulationStart = new DateOnly(today.Year, today.Month, 1);

        var birthYear = today.Year - CurrentAge;
        var dateOfBirth = new DateOnly(birthYear, 5, 15);
        var retirementDate = new DateOnly(birthYear + RetirementAge, 6, 1);
        var simulationEnd = new DateOnly(birthYear + LifeExpectancy, 5, 1);

        var parameters = new SimulationParametersDto
        {
            SimulationStart = simulationStart,
            SimulationEnd = simulationEnd,
            DateOfBirth = dateOfBirth,
            RetirementDate = retirementDate,
            InitialLiquidCash = 15_000m,
            CurrencyCode = Currency
        };

        return new DefaultTimeContext(now, parameters);
    }
}
