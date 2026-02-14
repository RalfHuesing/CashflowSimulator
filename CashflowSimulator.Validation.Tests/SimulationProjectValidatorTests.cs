using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using CashflowSimulator.Validation.Tests.TestData;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class SimulationProjectValidatorTests
{
    [Fact]
    public void Validate_ValidProject_ReturnsIsValid()
    {
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = new SimulationParametersDtoBuilder().Build(),
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_InvalidMeta_ReturnsErrors()
    {
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().WithScenarioName("").Build(),
            Parameters = new SimulationParametersDtoBuilder().Build(),
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ScenarioName" || e.PropertyName.StartsWith("Meta."));
    }

    [Fact]
    public void Validate_InvalidParameters_ReturnsErrors()
    {
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = new SimulationParametersDtoBuilder().WithRetirementBeforeBirth().Build(),
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 1);
    }

    [Fact]
    public void Validate_NullMeta_ReturnsErrors()
    {
        var dto = new SimulationProjectDto
        {
            Meta = null!,
            Parameters = new SimulationParametersDtoBuilder().Build(),
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NullParameters_ReturnsErrors()
    {
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = null!,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
    }
}
