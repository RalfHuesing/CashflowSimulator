using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

/// <summary>
/// Unit-Tests für <see cref="Validators.EconomicFactorDtoValidator"/>:
/// Id/Name Pflicht und Länge, ExpectedReturn/Volatility/MeanReversionSpeed/InitialValue Grenzen.
/// </summary>
public sealed class EconomicFactorDtoValidatorTests
{
    private static EconomicFactorDto ValidDto() => new()
    {
        Id = "MSCI_World",
        Name = "MSCI World",
        ModelType = StochasticModelType.GeometricBrownianMotion,
        ExpectedReturn = 0.07,
        Volatility = 0.15,
        MeanReversionSpeed = 0,
        InitialValue = 100
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
        Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.Message.Contains("ID"));
    }

    [Fact]
    public void Validate_IdExceedsMaxLength_ReturnsError()
    {
        var dto = ValidDto() with { Id = new string('x', 101) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.Message.Contains("100"));
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
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.Message.Contains("Leerzeichen"));
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_ReturnsError()
    {
        var dto = ValidDto() with { Name = new string('a', 201) };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.Message.Contains("200"));
    }

    [Theory]
    [InlineData(-0.51)]
    [InlineData(0.51)]
    public void Validate_ExpectedReturnOutOfRange_ReturnsError(double value)
    {
        var dto = ValidDto() with { ExpectedReturn = value };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpectedReturn");
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(0.5)]
    public void Validate_ExpectedReturnAtBoundary_ReturnsIsValid(double value)
    {
        var dto = ValidDto() with { ExpectedReturn = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0.0009)]
    [InlineData(2.01)]
    public void Validate_VolatilityOutOfRange_ReturnsError(double value)
    {
        var dto = ValidDto() with { Volatility = value };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Volatility");
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(2.0)]
    public void Validate_VolatilityAtBoundary_ReturnsIsValid(double value)
    {
        var dto = ValidDto() with { Volatility = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(10.01)]
    public void Validate_MeanReversionSpeedOutOfRange_ReturnsError(double value)
    {
        var dto = ValidDto() with { MeanReversionSpeed = value };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MeanReversionSpeed");
    }

    [Fact]
    public void Validate_InitialValueZero_ReturnsError()
    {
        var dto = ValidDto() with { InitialValue = 0 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InitialValue" && e.Message.Contains("größer als 0"));
    }

    [Fact]
    public void Validate_InitialValueNegative_ReturnsError()
    {
        var dto = ValidDto() with { InitialValue = -1 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InitialValue");
    }

    [Fact]
    public void Validate_InitialValueBelowMin_ReturnsError()
    {
        var dto = ValidDto() with { InitialValue = 1e-7 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InitialValue");
    }

    [Fact]
    public void Validate_InitialValueAboveMax_ReturnsError()
    {
        var dto = ValidDto() with { InitialValue = 1e6 + 1 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InitialValue");
    }

    [Theory]
    [InlineData(1e-6)]
    [InlineData(1e6)]
    public void Validate_InitialValueAtBoundary_ReturnsIsValid(double value)
    {
        var dto = ValidDto() with { InitialValue = value };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
