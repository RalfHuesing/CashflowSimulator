using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Erzeugt Standard-Steuer- und Strategie-Profile, Allokationsprofile sowie zwei Lebensphasen (Anspar, Rente).
/// </summary>
internal static class DefaultLifecycleDataBuilder
{
    private const int RetirementAge = 67;

    public static (List<TaxProfileDto> TaxProfiles, List<StrategyProfileDto> StrategyProfiles, List<AllocationProfileDto> AllocationProfiles, List<LifecyclePhaseDto> LifecyclePhases)
        Build(SimulationParametersDto parameters, List<AssetClassDto> assetClasses)
    {
        var taxStandardId = Guid.NewGuid().ToString();
        var taxRetirementId = Guid.NewGuid().ToString();
        var strategyBuildId = Guid.NewGuid().ToString();
        var strategyWithdrawId = Guid.NewGuid().ToString();
        var profileBuildId = Guid.NewGuid().ToString();
        var profileWithdrawId = Guid.NewGuid().ToString();

        var idAktien = assetClasses.First(c => c.Id == "Aktien_Welt").Id;
        var idSchwellen = assetClasses.First(c => c.Id == "Schwellenlaender").Id;
        var idSicherheit = assetClasses.First(c => c.Id == "Sicherheitsbaustein").Id;

        var allocationProfiles = new List<AllocationProfileDto>
        {
            new()
            {
                Id = profileBuildId,
                Name = "Aufbau",
                Entries =
                [
                    new AllocationProfileEntryDto { AssetClassId = idAktien, TargetWeight = 0.70m },
                    new AllocationProfileEntryDto { AssetClassId = idSchwellen, TargetWeight = 0.10m },
                    new AllocationProfileEntryDto { AssetClassId = idSicherheit, TargetWeight = 0.20m }
                ]
            },
            new()
            {
                Id = profileWithdrawId,
                Name = "Rente",
                Entries =
                [
                    new AllocationProfileEntryDto { AssetClassId = idAktien, TargetWeight = 0.60m },
                    new AllocationProfileEntryDto { AssetClassId = idSchwellen, TargetWeight = 0m },
                    new AllocationProfileEntryDto { AssetClassId = idSicherheit, TargetWeight = 0.40m }
                ]
            }
        };

        var taxProfiles = new List<TaxProfileDto>
        {
            new()
            {
                Id = taxStandardId,
                Name = "Standard (Erwerb)",
                CapitalGainsTaxRate = 0.26375m,
                TaxFreeAllowance = 1000m,
                IncomeTaxRate = 0.35m
            },
            new()
            {
                Id = taxRetirementId,
                Name = "Rentenbesteuerung",
                CapitalGainsTaxRate = 0.26375m,
                TaxFreeAllowance = 1000m,
                IncomeTaxRate = 0.18m
            }
        };

        var strategyProfiles = new List<StrategyProfileDto>
        {
            new()
            {
                Id = strategyBuildId,
                Name = "Aufbau",
                CashReserveMonths = 3,
                RebalancingThreshold = 0.05m,
                MinimumTransactionAmount = 50m,
                LookaheadMonths = 24
            },
            new()
            {
                Id = strategyWithdrawId,
                Name = "Entnahme",
                CashReserveMonths = 12,
                RebalancingThreshold = 0.05m,
                MinimumTransactionAmount = 50m,
                LookaheadMonths = 6
            }
        };

        var ageAtStart = GetAgeInYears(parameters.DateOfBirth, parameters.SimulationStart);

        var lifecyclePhases = new List<LifecyclePhaseDto>
        {
            new()
            {
                StartAge = ageAtStart,
                TaxProfileId = taxStandardId,
                StrategyProfileId = strategyBuildId,
                AssetAllocationOverrides = [],
                AllocationProfileId = profileBuildId,
                GlidepathMonths = 0
            },
            new()
            {
                StartAge = RetirementAge,
                TaxProfileId = taxRetirementId,
                StrategyProfileId = strategyWithdrawId,
                AssetAllocationOverrides = [],
                AllocationProfileId = profileWithdrawId,
                GlidepathMonths = 60
            }
        };

        return (taxProfiles, strategyProfiles, allocationProfiles, lifecyclePhases);
    }

    private static int GetAgeInYears(DateOnly dateOfBirth, DateOnly atDate)
    {
        var years = atDate.Year - dateOfBirth.Year;
        if (dateOfBirth.AddYears(years) > atDate)
            years--;
        return years;
    }
}
