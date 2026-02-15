using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für das komplette <see cref="SimulationProjectDto"/>.
/// Bündelt Meta- und Parameters-Validierung; prüft EconomicFactors, Correlations und Korrelationsmatrix.
/// </summary>
public sealed class SimulationProjectValidator : AbstractValidator<SimulationProjectDto>
{
    public SimulationProjectValidator()
    {
        RuleFor(x => x.Meta)
            .NotNull()
            .SetValidator(new MetaDtoValidator());

        RuleFor(x => x.Parameters)
            .NotNull()
            .SetValidator(new SimulationParametersValidator());

        RuleFor(x => x.UiSettings)
            .NotNull();

        RuleForEach(x => x.EconomicFactors)
            .SetValidator(new EconomicFactorDtoValidator());

        RuleForEach(x => x.Correlations)
            .SetValidator(new CorrelationEntryDtoValidator());

        RuleForEach(x => x.Correlations)
            .Must((project, entry) =>
            {
                var ids = project.EconomicFactors?.Select(f => f.Id).ToHashSet() ?? new HashSet<string>();
                return ids.Contains(entry.FactorIdA) && ids.Contains(entry.FactorIdB);
            })
            .WithMessage("Faktor A und B müssen existierende Marktfaktoren sein.");

        RuleFor(x => x)
            .Must(project => CorrelationMatrixValidation.GetPositiveDefinitenessError(project) is null)
            .WithMessage(project => CorrelationMatrixValidation.GetPositiveDefinitenessError(project) ?? "Korrelationsmatrix ungültig.");

        RuleForEach(x => x.TaxProfiles)
            .SetValidator(new TaxProfileDtoValidator());

        RuleForEach(x => x.StrategyProfiles)
            .SetValidator(new StrategyProfileDtoValidator());

        RuleForEach(x => x.LifecyclePhases)
            .SetValidator(new LifecyclePhaseDtoValidator());

        RuleForEach(x => x.LifecyclePhases)
            .Must((project, phase) =>
            {
                var taxIds = project.TaxProfiles?.Select(p => p.Id).ToHashSet() ?? new HashSet<string>();
                var strategyIds = project.StrategyProfiles?.Select(p => p.Id).ToHashSet() ?? new HashSet<string>();
                return taxIds.Contains(phase.TaxProfileId) && strategyIds.Contains(phase.StrategyProfileId);
            })
            .WithMessage("Jede Lebensphase muss auf existierende Steuer- und Strategie-Profile verweisen (TaxProfileId, StrategyProfileId).");

        RuleFor(x => x)
            .Must(project =>
            {
                if (project.LifecyclePhases is null || project.LifecyclePhases.Count == 0)
                    return false;
                if (project.Parameters is null || project.Parameters.SimulationStart == default || project.Parameters.DateOfBirth == default)
                    return true; // andere Regeln melden Parameter-Fehler
                var ageAtStart = GetAgeInYears(project.Parameters.DateOfBirth, project.Parameters.SimulationStart);
                return project.LifecyclePhases.Any(p => p.StartAge == 0 || p.StartAge <= ageAtStart);
            })
            .WithMessage("Es muss mindestens eine Lebensphase geben, die zum Simulationsstart aktiv ist (StartAge 0 oder StartAge ≤ Alter zum Start).");

        RuleForEach(x => x.Portfolio.Assets)
            .Must((project, asset) =>
            {
                if (string.IsNullOrEmpty(asset.EconomicFactorId))
                    return false;
                var factorIds = project.EconomicFactors?.Select(f => f.Id).ToHashSet() ?? new HashSet<string>();
                return factorIds.Contains(asset.EconomicFactorId);
            })
            .WithMessage("Jedes Asset muss auf einen existierenden Marktfaktor verweisen (EconomicFactorId).");
    }

    private static int GetAgeInYears(DateOnly dateOfBirth, DateOnly atDate)
    {
        var years = atDate.Year - dateOfBirth.Year;
        if (dateOfBirth.AddYears(years) > atDate)
            years--;
        return years;
    }
}
