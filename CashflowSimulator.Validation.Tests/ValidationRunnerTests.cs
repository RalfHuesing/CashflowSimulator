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
}
