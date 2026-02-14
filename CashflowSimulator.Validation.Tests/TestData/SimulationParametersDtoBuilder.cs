using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Validation.Tests.TestData;

/// <summary>
/// Builder für gültige und ungültige <see cref="SimulationParametersDto"/>-Instanzen in Tests.
/// Hält Konventionen zentral (1. des Monats, sinnvolle Altersbereiche).
/// </summary>
public sealed class SimulationParametersDtoBuilder
{
    private DateOnly _simulationStart = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    private DateOnly _dateOfBirth = new(1980, 1, 1);
    private DateOnly _retirementDate = new(2047, 1, 1);  // Alter 67
    private DateOnly _simulationEnd = new(2075, 1, 1);    // Alter 95
    private decimal _initialLiquidCash = 0;
    private string _currencyCode = "EUR";

    public SimulationParametersDtoBuilder WithDateOfBirth(DateOnly value)
    {
        _dateOfBirth = value;
        return this;
    }

    public SimulationParametersDtoBuilder WithRetirementDate(DateOnly value)
    {
        _retirementDate = value;
        return this;
    }

    public SimulationParametersDtoBuilder WithSimulationEnd(DateOnly value)
    {
        _simulationEnd = value;
        return this;
    }

    public SimulationParametersDtoBuilder WithInitialLiquidCash(decimal value)
    {
        _initialLiquidCash = value;
        return this;
    }

    public SimulationParametersDtoBuilder WithCurrencyCode(string value)
    {
        _currencyCode = value ?? "EUR";
        return this;
    }

    /// <summary>
    /// Setzt Renteneintritt vor Geburt (ungültig).
    /// </summary>
    public SimulationParametersDtoBuilder WithRetirementBeforeBirth()
    {
        _retirementDate = _dateOfBirth.AddYears(-1);
        return this;
    }

    /// <summary>
    /// Setzt Lebenserwartung vor Renteneintritt (ungültig).
    /// </summary>
    public SimulationParametersDtoBuilder WithLifeExpectancyBeforeRetirement()
    {
        _simulationEnd = _retirementDate.AddYears(-1);
        return this;
    }

    /// <summary>
    /// Renteneintrittsalter unter 50 (ungültig).
    /// </summary>
    public SimulationParametersDtoBuilder WithRetirementAgeTooLow()
    {
        _retirementDate = _dateOfBirth.AddYears(40);
        return this;
    }

    /// <summary>
    /// Renteneintrittsalter über 75 (ungültig).
    /// </summary>
    public SimulationParametersDtoBuilder WithRetirementAgeTooHigh()
    {
        _retirementDate = _dateOfBirth.AddYears(80);
        return this;
    }

    /// <summary>
    /// Lebenserwartung unter 60 (ungültig).
    /// </summary>
    public SimulationParametersDtoBuilder WithLifeExpectancyTooLow()
    {
        _simulationEnd = _dateOfBirth.AddYears(55);
        return this;
    }

    /// <summary>
    /// Lebenserwartung über 120 (ungültig).
    /// </summary>
    public SimulationParametersDtoBuilder WithLifeExpectancyTooHigh()
    {
        _simulationEnd = _dateOfBirth.AddYears(125);
        return this;
    }

    public SimulationParametersDto Build() => new()
    {
        SimulationStart = _simulationStart,
        SimulationEnd = _simulationEnd,
        DateOfBirth = _dateOfBirth,
        RetirementDate = _retirementDate,
        InitialLiquidCash = _initialLiquidCash,
        CurrencyCode = _currencyCode
    };
}
