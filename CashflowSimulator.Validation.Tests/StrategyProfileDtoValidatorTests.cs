using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class StrategyProfileDtoValidatorTests
{
    private static StrategyProfileDto ValidDto() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Aufbau",
        CashReserveMonths = 3,
        RebalancingThreshold = 0.05m,
        LookaheadMonths = 24
    };

    [Fact]
    public void Validate_ValidDto_ReturnsIsValid()
    {
        var result = ValidationRunner.Validate(ValidDto());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyId_ReturnsError()
    {
        var dto = ValidDto() with { Id = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.Message.Contains("Strategie-Profil"));
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var dto = ValidDto() with { Name = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NegativeCashReserveMonths_ReturnsError()
    {
        var dto = ValidDto() with { CashReserveMonths = -1 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CashReserveMonths");
    }

    [Fact]
    public void Validate_RebalancingThresholdOutOfRange_ReturnsError()
    {
        var dto = ValidDto() with { RebalancingThreshold = 1.5m };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RebalancingThreshold");
    }

    [Fact]
    public void Validate_NegativeMinimumTransactionAmount_ReturnsError()
    {
        var dto = ValidDto() with { MinimumTransactionAmount = -10m };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MinimumTransactionAmount");
    }

    [Fact]
    public void Validate_NegativeLookaheadMonths_ReturnsError()
    {
        var dto = ValidDto() with { LookaheadMonths = -1 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LookaheadMonths");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.05)]
    [InlineData(1)]
    public void Validate_RebalancingThresholdInRange_ReturnsIsValid(decimal value)
    {
        var dto = ValidDto() with { RebalancingThreshold = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
