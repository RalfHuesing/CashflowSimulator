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
    public void Validate_WhitespaceOnlyName_ReturnsError()
    {
        var dto = ValidDto() with { Name = "   " };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.Message.Contains("Leerzeichen"));
    }

    [Theory]
    [InlineData(201)]
    [InlineData(250)]
    public void Validate_NameExceedsMaxLength_ReturnsError(int length)
    {
        var dto = ValidDto() with { Name = new string('x', length) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.Message.Contains("200"));
    }

    [Fact]
    public void Validate_NameExactly200Chars_ReturnsIsValid()
    {
        var dto = ValidDto() with { Name = new string('a', 200) };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
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
    public void Validate_StartDateYearBefore1900_ReturnsError()
    {
        var dto = ValidDto() with { StartDate = new DateOnly(1899, 6, 15) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate" && e.Message.Contains("1900"));
    }

    [Fact]
    public void Validate_StartDateYearAfter2100_ReturnsError()
    {
        var dto = ValidDto() with { StartDate = new DateOnly(2101, 1, 1) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate" && e.Message.Contains("2100"));
    }

    [Theory]
    [InlineData(1900)]
    [InlineData(2100)]
    public void Validate_StartDateBoundaryYears_ReturnsIsValid(int year)
    {
        var dto = ValidDto() with { StartDate = new DateOnly(year, 6, 15) };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NegativeAmount_ReturnsError()
    {
        var dto = ValidDto() with { Amount = -100 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount" && e.Message.Contains("Betrag"));
    }

    [Fact]
    public void Validate_ZeroAmount_ReturnsError()
    {
        var dto = ValidDto() with { Amount = 0 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_EmptyInterval_ReturnsError()
    {
        var dto = ValidDto() with { Interval = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Interval");
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
    public void Validate_YearlyInterval_ReturnsIsValid()
    {
        var dto = ValidDto() with { Interval = "Yearly" };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
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
    public void Validate_EndDateYearBefore1900_ReturnsError()
    {
        var start = new DateOnly(2000, 1, 1);
        var dto = ValidDto() with { StartDate = start, EndDate = new DateOnly(1899, 12, 31) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.Message.Contains("1900"));
    }

    [Fact]
    public void Validate_EndDateSameAsStartDate_ReturnsIsValid()
    {
        var start = DateOnly.FromDateTime(DateTime.Today);
        var dto = ValidDto() with { StartDate = start, EndDate = start };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AllBoundariesValid_ReturnsIsValid()
    {
        var dto = ValidDto() with
        {
            Name = new string('a', 200),
            StartDate = new DateOnly(2020, 1, 1),
            EndDate = new DateOnly(2100, 12, 31),
            Amount = 0.01m,
            Interval = "Yearly"
        };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
