using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="AllocationProfileEntryDto"/>.
/// Wird innerhalb von <see cref="AllocationProfileDtoValidator"/> für Einträge verwendet.
/// </summary>
public sealed class AllocationProfileEntryDtoValidator : AbstractValidator<AllocationProfileEntryDto>
{
    private const decimal WeightMin = 0;
    private const decimal WeightMax = 1;

    public AllocationProfileEntryDtoValidator()
    {
        RuleFor(x => x.AssetClassId)
            .NotEmpty()
            .WithMessage("Anlageklassen-ID muss angegeben werden.");

        RuleFor(x => x.TargetWeight)
            .InclusiveBetween(WeightMin, WeightMax)
            .WithMessage($"Zielgewichtung muss zwischen {WeightMin} und {WeightMax} liegen (z. B. 0,3 = 30 %).");
    }
}
