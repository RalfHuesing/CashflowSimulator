using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class TaxProfileDtoValidatorTests
{
    private static TaxProfileDto ValidDto() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Standard",
        CapitalGainsTaxRate = 0.26375m,
        TaxFreeAllowance = 1000m,
        IncomeTaxRate = 0.35m
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
        Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.Message.Contains("Steuer-Profil"));
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
    public void Validate_WhitespaceOnlyName_ReturnsError()
    {
        var dto = ValidDto() with { Name = "   " };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Validate_CapitalGainsTaxRateOutOfRange_ReturnsError(decimal value)
    {
        var dto = ValidDto() with { CapitalGainsTaxRate = value };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CapitalGainsTaxRate");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(0.26375)]
    public void Validate_CapitalGainsTaxRateInRange_ReturnsIsValid(decimal value)
    {
        var dto = ValidDto() with { CapitalGainsTaxRate = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NegativeTaxFreeAllowance_ReturnsError()
    {
        var dto = ValidDto() with { TaxFreeAllowance = -1m };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TaxFreeAllowance");
    }

    [Fact]
    public void Validate_IncomeTaxRateOutOfRange_ReturnsError()
    {
        var dto = ValidDto() with { IncomeTaxRate = 1.5m };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IncomeTaxRate");
    }
}
