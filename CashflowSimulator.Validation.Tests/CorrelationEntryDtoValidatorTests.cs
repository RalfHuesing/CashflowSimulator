using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

/// <summary>
/// Unit-Tests für <see cref="Validators.CorrelationEntryDtoValidator"/>:
/// FactorIdA/B Pflicht und unterschiedlich, Korrelation in [-1, 1].
/// </summary>
public sealed class CorrelationEntryDtoValidatorTests
{
    private static CorrelationEntryDto ValidDto() => new()
    {
        FactorIdA = "A",
        FactorIdB = "B",
        Correlation = 0.3
    };

    [Fact]
    public void Validate_ValidDto_ReturnsIsValid()
    {
        var result = ValidationRunner.Validate(ValidDto());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyFactorIdA_ReturnsError()
    {
        var dto = ValidDto() with { FactorIdA = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FactorIdA");
    }

    [Fact]
    public void Validate_EmptyFactorIdB_ReturnsError()
    {
        var dto = ValidDto() with { FactorIdB = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FactorIdB");
    }

    [Fact]
    public void Validate_FactorIdASameAsFactorIdB_ReturnsError()
    {
        var dto = ValidDto() with { FactorIdA = "X", FactorIdB = "X" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("unterschiedlich"));
    }

    [Theory]
    [InlineData(-1.1)]
    [InlineData(1.1)]
    public void Validate_CorrelationOutOfRange_ReturnsError(double value)
    {
        var dto = ValidDto() with { Correlation = value };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Correlation" && e.Message.Contains("-1") && e.Message.Contains("1"));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(1.0)]
    [InlineData(0)]
    public void Validate_CorrelationAtBoundary_ReturnsIsValid(double value)
    {
        var dto = ValidDto() with { Correlation = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
