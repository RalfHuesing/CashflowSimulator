using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using CashflowSimulator.Validation.Tests.TestData;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class SimulationProjectValidatorTests
{
    /// <summary>
    /// Liefert minimale gültige Lifecycle-Daten (eine Phase mit StartAge 0, die immer zum Simulationsstart passt).
    /// </summary>
    private static (List<TaxProfileDto> TaxProfiles, List<StrategyProfileDto> StrategyProfiles, List<LifecyclePhaseDto> LifecyclePhases) ValidLifecycleData()
    {
        var taxId = Guid.NewGuid().ToString();
        var strategyId = Guid.NewGuid().ToString();
        var taxProfiles = new List<TaxProfileDto>
        {
            new() { Id = taxId, Name = "Standard", CapitalGainsTaxRate = 0.26375m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.35m }
        };
        var strategyProfiles = new List<StrategyProfileDto>
        {
            new() { Id = strategyId, Name = "Aufbau", CashReserveMonths = 3, RebalancingThreshold = 0.05m, LookaheadMonths = 24 }
        };
        var lifecyclePhases = new List<LifecyclePhaseDto>
        {
            new() { StartAge = 0, TaxProfileId = taxId, StrategyProfileId = strategyId, AssetAllocationOverrides = [] }
        };
        return (taxProfiles, strategyProfiles, lifecyclePhases);
    }

    [Fact]
    public void Validate_ValidProject_ReturnsIsValid()
    {
        var (taxProfiles, strategyProfiles, lifecyclePhases) = ValidLifecycleData();
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = new SimulationParametersDtoBuilder().Build(),
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = lifecyclePhases,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_InvalidMeta_ReturnsErrors()
    {
        var (taxProfiles, strategyProfiles, lifecyclePhases) = ValidLifecycleData();
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().WithScenarioName("").Build(),
            Parameters = new SimulationParametersDtoBuilder().Build(),
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = lifecyclePhases,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ScenarioName" || e.PropertyName.StartsWith("Meta."));
    }

    [Fact]
    public void Validate_InvalidParameters_ReturnsErrors()
    {
        var (taxProfiles, strategyProfiles, lifecyclePhases) = ValidLifecycleData();
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = new SimulationParametersDtoBuilder().WithRetirementBeforeBirth().Build(),
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = lifecyclePhases,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 1);
    }

    [Fact]
    public void Validate_NullMeta_ReturnsErrors()
    {
        var (taxProfiles, strategyProfiles, lifecyclePhases) = ValidLifecycleData();
        var dto = new SimulationProjectDto
        {
            Meta = null!,
            Parameters = new SimulationParametersDtoBuilder().Build(),
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = lifecyclePhases,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NullParameters_ReturnsErrors()
    {
        var (taxProfiles, strategyProfiles, lifecyclePhases) = ValidLifecycleData();
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = null!,
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = lifecyclePhases,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyLifecyclePhases_ReturnsError()
    {
        var (taxProfiles, strategyProfiles, _) = ValidLifecycleData();
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = new SimulationParametersDtoBuilder().Build(),
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = [],
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Lebensphase") || e.Message.Contains("Simulationsstart"));
    }

    [Fact]
    public void Validate_PhaseReferencesNonExistentTaxProfileId_ReturnsError()
    {
        var strategyId = Guid.NewGuid().ToString();
        var taxProfiles = new List<TaxProfileDto>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Standard", CapitalGainsTaxRate = 0.26m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.35m }
        };
        var strategyProfiles = new List<StrategyProfileDto>
        {
            new() { Id = strategyId, Name = "Aufbau", CashReserveMonths = 3, RebalancingThreshold = 0.05m, LookaheadMonths = 24 }
        };
        var lifecyclePhases = new List<LifecyclePhaseDto>
        {
            new() { StartAge = 0, TaxProfileId = "non-existent-id", StrategyProfileId = strategyId, AssetAllocationOverrides = [] }
        };
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = new SimulationParametersDtoBuilder().Build(),
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = lifecyclePhases,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("existierende") || e.Message.Contains("TaxProfileId") || e.Message.Contains("StrategyProfileId"));
    }

    [Fact]
    public void Validate_NoPhaseCoversSimulationStart_ReturnsError()
    {
        var taxId = Guid.NewGuid().ToString();
        var strategyId = Guid.NewGuid().ToString();
        // Alter zum Start = 30 (1990 → 2020). Nur Phase ab 67 = keine Phase deckt Start ab.
        var parameters = new SimulationParametersDtoBuilder()
            .WithDateOfBirth(new DateOnly(1990, 1, 1))
            .WithRetirementDate(new DateOnly(2057, 1, 1))
            .WithSimulationEnd(new DateOnly(2085, 1, 1))
            .Build();
        parameters = parameters with { SimulationStart = new DateOnly(2020, 1, 1) };
        var taxProfiles = new List<TaxProfileDto>
        {
            new() { Id = taxId, Name = "Rente", CapitalGainsTaxRate = 0.26m, TaxFreeAllowance = 1000m, IncomeTaxRate = 0.18m }
        };
        var strategyProfiles = new List<StrategyProfileDto>
        {
            new() { Id = strategyId, Name = "Entnahme", CashReserveMonths = 12, RebalancingThreshold = 0.05m, LookaheadMonths = 6 }
        };
        var lifecyclePhases = new List<LifecyclePhaseDto>
        {
            new() { StartAge = 67, TaxProfileId = taxId, StrategyProfileId = strategyId, AssetAllocationOverrides = [] }
        };
        var dto = new SimulationProjectDto
        {
            Meta = new MetaDtoBuilder().Build(),
            Parameters = parameters,
            TaxProfiles = taxProfiles,
            StrategyProfiles = strategyProfiles,
            LifecyclePhases = lifecyclePhases,
            UiSettings = new UiSettingsDto()
        };
        var result = ValidationRunner.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Simulationsstart") || e.Message.Contains("StartAge"));
    }
}
