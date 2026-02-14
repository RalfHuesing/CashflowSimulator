using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class CashflowStreamDtoValidatorTests
{
    private static CashflowStreamDto ValidDto() => new()
    {
        Name = "Gehalt",
        Type = CashflowType.Income,
        Amount = 1000,
        Interval = "Monthly",
        StartDate = DateOnly.FromDateTime(DateTime.Today)
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
    public void Validate_DefaultStartDate_ReturnsError()
    {
        var dto = ValidDto() with { StartDate = default };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate" && e.Message.Contains("Startdatum"));
    }

    [Fact]
    public void Validate_NegativeAmount_ReturnsError()
    {
        var dto = ValidDto() with { Amount = -100 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount" && e.Message.Contains("negativ"));
    }

    [Fact]
    public void Validate_InvalidInterval_ReturnsError()
    {
        var dto = ValidDto() with { Interval = "Weekly" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Interval");
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_ReturnsError()
    {
        var start = DateOnly.FromDateTime(DateTime.Today);
        var dto = ValidDto() with { StartDate = start, EndDate = start.AddDays(-1) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Enddatum"));
    }

    [Fact]
    public void Validate_YearlyInterval_ReturnsIsValid()
    {
        var dto = ValidDto() with { Interval = "Yearly" };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
