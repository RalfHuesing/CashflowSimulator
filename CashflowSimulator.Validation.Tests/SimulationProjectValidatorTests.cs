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
        var dto = new SimulationProjectDtoBuilder().Build();
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_InvalidMeta_ReturnsErrors()
    {
        var dto = new SimulationProjectDtoBuilder()
            .WithMeta(new MetaDtoBuilder().WithScenarioName("").Build())
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ScenarioName" || e.PropertyName.StartsWith("Meta."));
    }

    [Fact]
    public void Validate_InvalidParameters_ReturnsErrors()
    {
        var dto = new SimulationProjectDtoBuilder()
            .WithParameters(new SimulationParametersDtoBuilder().WithSimulationEndBeforeBirth().Build())
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 1);
    }

    [Fact]
    public void Validate_NullMeta_ReturnsErrors()
    {
        var dto = new SimulationProjectDtoBuilder().Build();
        dto = dto with { Meta = null! };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NullParameters_ReturnsErrors()
    {
        var dto = new SimulationProjectDtoBuilder().Build();
        dto = dto with { Parameters = null! };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyLifecyclePhases_ReturnsError()
    {
        var dto = new SimulationProjectDtoBuilder().WithEmptyLifecyclePhases().Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Lebensphase") || e.Message.Contains("Simulationsstart"));
    }

    [Fact]
    public void Validate_PhaseReferencesNonExistentTaxProfileId_ReturnsError()
    {
        var dto = new SimulationProjectDtoBuilder()
            .WithPhaseReferencingNonExistentTaxProfile("non-existent-id")
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("existierende") || e.Message.Contains("TaxProfileId") || e.Message.Contains("StrategyProfileId"));
    }

    [Fact]
    public void Validate_NoPhaseCoversSimulationStart_ReturnsError()
    {
        var dto = new SimulationProjectDtoBuilder().WithNoPhaseCoveringSimulationStart().Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Simulationsstart") || e.Message.Contains("StartAge"));
    }

    [Fact]
    public void Validate_NullUiSettings_ReturnsError()
    {
        var dto = new SimulationProjectDtoBuilder().Build();
        dto = dto with { UiSettings = null! };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UiSettings");
    }

    [Fact]
    public void Validate_CorrelationsReferenceNonExistentFactor_ReturnsError()
    {
        var dto = new SimulationProjectDtoBuilder()
            .WithEconomicFactors(
                [new EconomicFactorDto { Id = "A", Name = "Faktor A", ExpectedReturn = 0.05, Volatility = 0.1, InitialValue = 100 }],
                [new CorrelationEntryDto { FactorIdA = "A", FactorIdB = "NonExistent", Correlation = 0.5 }])
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("existierende Marktfaktoren") || e.Message.Contains("Faktor"));
    }

    [Fact]
    public void Validate_AssetWithNonExistentEconomicFactorId_ReturnsError()
    {
        var dto = new SimulationProjectDtoBuilder()
            .WithPortfolio(new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "ETF",
                        EconomicFactorId = "NonExistentFactor",
                        AssetClassId = "ac1",
                        CurrentPrice = 100,
                        CurrentQuantity = 10
                    }
                ]
            })
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("existierenden Marktfaktor") || e.Message.Contains("EconomicFactorId"));
    }

    [Fact]
    public void Validate_AssetWithValidEconomicFactorId_ReturnsIsValid()
    {
        var dto = new SimulationProjectDtoBuilder()
            .WithEconomicFactors(
                [new EconomicFactorDto { Id = "MSCI", Name = "MSCI World", ExpectedReturn = 0.07, Volatility = 0.15, InitialValue = 100 }],
                null)
            .WithPortfolio(new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "ETF",
                        EconomicFactorId = "MSCI",
                        AssetClassId = "ac1",
                        CurrentPrice = 100,
                        CurrentQuantity = 10
                    }
                ]
            })
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AssetWithEmptyEconomicFactorId_ReturnsError()
    {
        var dto = new SimulationProjectDtoBuilder()
            .WithPortfolio(new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "ETF",
                        EconomicFactorId = "",
                        AssetClassId = "ac1",
                        CurrentPrice = 100,
                        CurrentQuantity = 10
                    }
                ]
            })
            .Build();
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("existierenden Marktfaktor") || e.Message.Contains("EconomicFactorId"));
    }
}
