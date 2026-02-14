using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using CashflowSimulator.Validation.Tests.TestData;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

/// <summary>
/// Stellt sicher, dass ValidationRunner das Contracts-ValidationResult zurückgibt und das Mapping korrekt ist.
/// </summary>
public sealed class ValidationRunnerTests
{
    [Fact]
    public void ValidateParameters_ReturnsContractsValidationResultType()
    {
        var dto = new SimulationParametersDtoBuilder().Build();
        var result = ValidationRunner.Validate(dto);

        Assert.IsType<ValidationResult>(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateParameters_WhenInvalid_ErrorsAreContractsValidationError()
    {
        var dto = new SimulationParametersDtoBuilder().WithCurrencyCode("").Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        var first = result.Errors[0];
        Assert.IsType<ValidationError>(first);
        Assert.False(string.IsNullOrEmpty(first.Message));
    }

    [Fact]
    public void ValidateMeta_ReturnsContractsValidationResultType()
    {
        var dto = new MetaDtoBuilder().Build();
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateProject_ReturnsContractsValidationResultType()
    {
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = new SimulationParametersDtoBuilder().Build(),
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Success_ReturnsSameSuccessInstance()
    {
        var dto = new SimulationParametersDtoBuilder().Build();
        var a = ValidationRunner.Validate(dto);
        var b = ValidationRunner.Validate(dto);

        Assert.True(a.IsValid);
        Assert.True(b.IsValid);
        Assert.Same(a, b);
    }

    [Fact]
    public void Validate_CashflowStreamDto_ReturnsContractsValidationResultType()
    {
        var dto = new CashflowStreamDto
        {
            Name = "Gehalt",
            Type = CashflowType.Income,
            Amount = 1000,
            Interval = "Monthly",
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };
        var result = ValidationRunner.Validate(dto);

        Assert.IsType<ValidationResult>(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_CashflowStreamDto_WhenInvalid_ErrorsAreContractsValidationError()
    {
        var dto = new CashflowStreamDto
        {
            Name = "",
            Type = CashflowType.Income,
            Amount = 0,
            Interval = "Monthly",
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        var first = result.Errors[0];
        Assert.IsType<ValidationError>(first);
        Assert.False(string.IsNullOrEmpty(first.Message));
    }

    [Fact]
    public void Validate_CashflowEventDto_ReturnsContractsValidationResultType()
    {
        var dto = new CashflowEventDto
        {
            Name = "Bonus",
            Type = CashflowType.Income,
            Amount = 2000,
            TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1))
        };
        var result = ValidationRunner.Validate(dto);

        Assert.IsType<ValidationResult>(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_CashflowEventDto_WhenInvalid_ErrorsAreContractsValidationError()
    {
        var dto = new CashflowEventDto
        {
            Name = "",
            Type = CashflowType.Income,
            Amount = 0,
            TargetDate = default
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 1);
        var first = result.Errors[0];
        Assert.IsType<ValidationError>(first);
        Assert.False(string.IsNullOrEmpty(first.Message));
    }
}
