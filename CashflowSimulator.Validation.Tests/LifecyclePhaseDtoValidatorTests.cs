using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class LifecyclePhaseDtoValidatorTests
{
    private static LifecyclePhaseDto ValidDto() => new()
    {
        StartAge = 30,
        TaxProfileId = Guid.NewGuid().ToString(),
        StrategyProfileId = Guid.NewGuid().ToString(),
        AssetAllocationOverrides = []
    };

    [Fact]
    public void Validate_ValidDto_ReturnsIsValid()
    {
        var result = ValidationRunner.Validate(ValidDto());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_StartAgeZero_ReturnsIsValid()
    {
        var dto = ValidDto() with { StartAge = 0 };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NegativeStartAge_ReturnsError()
    {
        var dto = ValidDto() with { StartAge = -1 };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartAge");
    }

    [Fact]
    public void Validate_EmptyTaxProfileId_ReturnsError()
    {
        var dto = ValidDto() with { TaxProfileId = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TaxProfileId");
    }

    [Fact]
    public void Validate_EmptyStrategyProfileId_ReturnsError()
    {
        var dto = ValidDto() with { StrategyProfileId = "" };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StrategyProfileId");
    }

    [Fact]
    public void Validate_ValidAssetAllocationOverride_ReturnsIsValid()
    {
        var classId = Guid.NewGuid().ToString();
        var dto = ValidDto() with
        {
            AssetAllocationOverrides =
            [
                new AssetAllocationOverrideDto { AssetClassId = classId, TargetWeight = 0.3m }
            ]
        };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidAssetAllocationOverrideWeight_ReturnsError()
    {
        var dto = ValidDto() with
        {
            AssetAllocationOverrides =
            [
                new AssetAllocationOverrideDto { AssetClassId = Guid.NewGuid().ToString(), TargetWeight = 1.5m }
            ]
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("TargetWeight") || e.PropertyName.Contains("AssetAllocationOverrides"));
    }
}
