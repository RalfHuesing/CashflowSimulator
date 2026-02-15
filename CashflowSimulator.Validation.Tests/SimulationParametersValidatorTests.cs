using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using CashflowSimulator.Validation.Tests.TestData;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class SimulationParametersValidatorTests
{
    [Fact]
    public void Validate_ValidDto_ReturnsIsValid()
    {
        var dto = new SimulationParametersDtoBuilder().Build();
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_DefaultDateOfBirth_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithDateOfBirth(default)
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DateOfBirth" && e.Message.Contains("Geburtsdatum"));
    }

    [Fact]
    public void Validate_SimulationEndBeforeBirth_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithSimulationEndBeforeBirth()
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Lebenserwartung") || e.Message.Contains("Geburtsdatum"));
    }

    [Fact]
    public void Validate_LifeExpectancyBelow60_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithLifeExpectancyTooLow()
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("60") && e.Message.Contains("120"));
    }

    [Fact]
    public void Validate_LifeExpectancyAbove120_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithLifeExpectancyTooHigh()
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("60") && e.Message.Contains("120"));
    }

    [Fact]
    public void Validate_NegativeInitialLossCarryforwardGeneral_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithInitialLossCarryforwardGeneral(-100)
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InitialLossCarryforwardGeneral" && e.Message.Contains("negativ"));
    }

    [Fact]
    public void Validate_NegativeInitialLossCarryforwardStocks_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithInitialLossCarryforwardStocks(-50)
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InitialLossCarryforwardStocks" && e.Message.Contains("negativ"));
    }

    [Fact]
    public void Validate_NegativeInitialLiquidCash_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithInitialLiquidCash(-1000)
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InitialLiquidCash" && e.Message.Contains("negativ"));
    }

    [Fact]
    public void Validate_EmptyCurrencyCode_ReturnsError()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithCurrencyCode("")
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CurrencyCode");
    }

    [Fact]
    public void Validate_ValidDtoWithPositiveCash_ReturnsIsValid()
    {
        var dto = new SimulationParametersDtoBuilder()
            .WithInitialLiquidCash(50_000)
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
