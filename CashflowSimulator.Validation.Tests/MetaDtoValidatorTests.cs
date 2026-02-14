using CashflowSimulator.Validation;
using CashflowSimulator.Validation.Tests.TestData;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class MetaDtoValidatorTests
{
    [Fact]
    public void Validate_ValidDto_ReturnsIsValid()
    {
        var dto = new MetaDtoBuilder().Build();
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyScenarioName_ReturnsError()
    {
        var dto = new MetaDtoBuilder()
            .WithScenarioName("")
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ScenarioName" && e.Message.Contains("leer"));
    }

    [Fact]
    public void Validate_WhitespaceOnlyScenarioName_ReturnsError()
    {
        var dto = new MetaDtoBuilder()
            .WithScenarioName("   ")
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ScenarioName");
    }

    [Theory]
    [InlineData(201)]
    [InlineData(250)]
    public void Validate_ScenarioNameExceedsMaxLength_ReturnsError(int length)
    {
        var dto = new MetaDtoBuilder()
            .WithScenarioName(new string('x', length))
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ScenarioName" && e.Message.Contains("200"));
    }

    [Fact]
    public void Validate_ScenarioNameExactly200Chars_ReturnsIsValid()
    {
        var dto = new MetaDtoBuilder()
            .WithScenarioName(new string('a', 200))
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }
}
