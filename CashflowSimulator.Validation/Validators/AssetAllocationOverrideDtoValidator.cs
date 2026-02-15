using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="AssetAllocationOverrideDto"/>.
/// Wird innerhalb von <see cref="LifecyclePhaseDtoValidator"/> für Overrides verwendet.
/// </summary>
public sealed class AssetAllocationOverrideDtoValidator : AbstractValidator<AssetAllocationOverrideDto>
{
    private const decimal WeightMin = 0;
    private const decimal WeightMax = 1;

    public AssetAllocationOverrideDtoValidator()
    {
        RuleFor(x => x.AssetClassId)
            .NotEmpty()
            .WithMessage("Anlageklassen-ID muss angegeben werden.");

        RuleFor(x => x.TargetWeight)
            .InclusiveBetween(WeightMin, WeightMax)
            .WithMessage($"Zielgewichtung muss zwischen {WeightMin} und {WeightMax} liegen (z. B. 0,3 = 30 %).");
    }
}
