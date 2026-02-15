using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="LifecyclePhaseDto"/>.
/// StartAge >= 0, Pflicht-Referenzen auf Tax- und Strategie-Profil; Overrides optional validiert.
/// </summary>
public sealed class LifecyclePhaseDtoValidator : AbstractValidator<LifecyclePhaseDto>
{
    private const int MaxStartAge = 120;

    public LifecyclePhaseDtoValidator()
    {
        RuleFor(x => x.StartAge)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Startalter der Phase darf nicht negativ sein.")
            .LessThanOrEqualTo(MaxStartAge)
            .WithMessage($"Startalter darf maximal {MaxStartAge} Jahre betragen.");

        RuleFor(x => x.TaxProfileId)
            .NotEmpty()
            .WithMessage("Steuer-Profil (TaxProfileId) muss angegeben werden.");

        RuleFor(x => x.StrategyProfileId)
            .NotEmpty()
            .WithMessage("Strategie-Profil (StrategyProfileId) muss angegeben werden.");

        RuleForEach(x => x.AssetAllocationOverrides)
            .SetValidator(new AssetAllocationOverrideDtoValidator());
    }
}
