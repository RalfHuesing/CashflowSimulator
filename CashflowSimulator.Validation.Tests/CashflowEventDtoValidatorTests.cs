using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class CashflowEventDtoValidatorTests
{
    private static CashflowEventDto ValidDto() => new()
    {
        Name = "Bonus",
        Type = CashflowType.Income,
        Amount = 2000,
        TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1))
    };

    [Fact]
    public void Validate_ValidDto_ReturnsIsValid()
    {
        var result = ValidationRunner.Validate(ValidDto());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var dto = ValidDto() with { Name = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.Message.Contains("Name"));
    }

    [Fact]
    public void Validate_DefaultTargetDate_ReturnsError()
    {
        var dto = ValidDto() with { TargetDate = default };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetDate" && e.Message.Contains("Zieldatum"));
    }

    [Fact]
    public void Validate_NegativeAmount_ReturnsError()
    {
        var dto = ValidDto() with { Amount = -500 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount" && e.Message.Contains("negativ"));
    }

    [Fact]
    public void Validate_EarliestAfterLatest_ReturnsError()
    {
        var dto = ValidDto() with { EarliestMonthOffset = 10, LatestMonthOffset = -10 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Frühestens") || e.Message.Contains("Spätestens"));
    }

    [Fact]
    public void Validate_ValidOffsets_ReturnsIsValid()
    {
        var dto = ValidDto() with { EarliestMonthOffset = -12, LatestMonthOffset = 6 };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
