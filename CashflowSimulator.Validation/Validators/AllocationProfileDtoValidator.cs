using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="AllocationProfileDto"/>.
/// Pflichtfelder; Summe der Eintrags-Gewichte muss 1,0 (100 %) ergeben.
/// </summary>
public sealed class AllocationProfileDtoValidator : AbstractValidator<AllocationProfileDto>
{
    private const int NameMaxLength = 200;
    private const int IdMaxLength = 100;
    private const decimal WeightSumTolerance = 0.0001m;

    public AllocationProfileDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID des Allokationsprofils muss angegeben werden.")
            .MaximumLength(IdMaxLength)
            .WithMessage($"ID darf maximal {IdMaxLength} Zeichen haben.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name des Allokationsprofils muss angegeben werden.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Name darf nicht nur aus Leerzeichen bestehen.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name darf maximal {NameMaxLength} Zeichen haben.");

        RuleForEach(x => x.Entries)
            .SetValidator(new AllocationProfileEntryDtoValidator());

        RuleFor(x => x)
            .Must(profile =>
            {
                if (profile.Entries is null || profile.Entries.Count == 0)
                    return false;
                var sum = profile.Entries.Sum(e => e.TargetWeight);
                return Math.Abs(sum - 1.0m) <= WeightSumTolerance;
            })
            .WithMessage("Die Summe der Zielgewichtungen aller Einträge muss 1,0 (100 %) ergeben.");
    }
}
