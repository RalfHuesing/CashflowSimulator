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
    public void Validate_DefaultTargetDate_ReturnsError()
    {
        var dto = ValidDto() with { TargetDate = default };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetDate" && e.Message.Contains("Zieldatum"));
    }

    [Fact]
    public void Validate_TargetDateYearBefore1900_ReturnsError()
    {
        var dto = ValidDto() with { TargetDate = new DateOnly(1899, 6, 15) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetDate" && e.Message.Contains("1900"));
    }

    [Fact]
    public void Validate_TargetDateYearAfter2100_ReturnsError()
    {
        var dto = ValidDto() with { TargetDate = new DateOnly(2101, 1, 1) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetDate" && e.Message.Contains("2100"));
    }

    [Theory]
    [InlineData(1900)]
    [InlineData(2100)]
    public void Validate_TargetDateBoundaryYears_ReturnsIsValid(int year)
    {
        var dto = ValidDto() with { TargetDate = new DateOnly(year, 6, 15) };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NegativeAmount_ReturnsError()
    {
        var dto = ValidDto() with { Amount = -500 };
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
    public void Validate_EarliestMonthOffsetBelowMin_ReturnsError()
    {
        var dto = ValidDto() with { EarliestMonthOffset = -121 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EarliestMonthOffset" && e.Message.Contains("-120"));
    }

    [Fact]
    public void Validate_EarliestMonthOffsetAboveMax_ReturnsError()
    {
        var dto = ValidDto() with { EarliestMonthOffset = 1 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EarliestMonthOffset");
    }

    [Theory]
    [InlineData(-120)]
    [InlineData(0)]
    public void Validate_EarliestMonthOffsetBoundary_ReturnsIsValid(int value)
    {
        var dto = ValidDto() with { EarliestMonthOffset = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_LatestMonthOffsetNegative_ReturnsError()
    {
        var dto = ValidDto() with { LatestMonthOffset = -1 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LatestMonthOffset" && e.Message.Contains("0"));
    }

    [Fact]
    public void Validate_LatestMonthOffsetAboveMax_ReturnsError()
    {
        var dto = ValidDto() with { LatestMonthOffset = 121 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LatestMonthOffset" && e.Message.Contains("120"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void Validate_LatestMonthOffsetBoundary_ReturnsIsValid(int value)
    {
        var dto = ValidDto() with { LatestMonthOffset = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
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

    [Fact]
    public void Validate_AllBoundariesValid_ReturnsIsValid()
    {
        var dto = ValidDto() with
        {
            Name = new string('a', 200),
            TargetDate = new DateOnly(2050, 6, 15),
            Amount = 0.01m,
            EarliestMonthOffset = -120,
            LatestMonthOffset = 120
        };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
