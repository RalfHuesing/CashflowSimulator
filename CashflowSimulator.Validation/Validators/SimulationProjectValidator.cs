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
    }
}
