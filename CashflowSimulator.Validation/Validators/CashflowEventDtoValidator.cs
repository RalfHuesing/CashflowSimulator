using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="CashflowEventDto"/>.
/// </summary>
public sealed class CashflowEventDtoValidator : AbstractValidator<CashflowEventDto>
{
    private const int NameMaxLength = 200;
    private const int TargetDateMinYear = 1900;
    private const int TargetDateMaxYear = 2100;
    private const int EarliestMonthMin = -120;
    private const int EarliestMonthMax = 0;
    private const int LatestMonthMin = 0;
    private const int LatestMonthMax = 120;

    public CashflowEventDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name muss angegeben werden.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Name darf nicht nur aus Leerzeichen bestehen.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name darf maximal {NameMaxLength} Zeichen haben.");

        RuleFor(x => x.TargetDate)
            .Must(d => d != default)
            .WithMessage("Zieldatum muss angegeben werden.");
        RuleFor(x => x.TargetDate)
            .Must(d => d.Year >= TargetDateMinYear && d.Year <= TargetDateMaxYear)
            .WithMessage($"Zieldatum muss im Bereich {TargetDateMinYear}–{TargetDateMaxYear} liegen.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Betrag muss größer als 0 sein.");

        RuleFor(x => x.EarliestMonthOffset)
            .InclusiveBetween(EarliestMonthMin, EarliestMonthMax)
            .When(x => x.EarliestMonthOffset.HasValue)
            .WithMessage($"Frühestens (Monate) muss zwischen {EarliestMonthMin} und {EarliestMonthMax} liegen.");

        RuleFor(x => x.LatestMonthOffset)
            .InclusiveBetween(LatestMonthMin, LatestMonthMax)
            .When(x => x.LatestMonthOffset.HasValue)
            .WithMessage($"Spätestens (Monate) muss zwischen {LatestMonthMin} und {LatestMonthMax} liegen.");

        RuleFor(x => x)
            .Must(dto => !dto.EarliestMonthOffset.HasValue || !dto.LatestMonthOffset.HasValue ||
                         dto.EarliestMonthOffset <= dto.LatestMonthOffset)
            .WithMessage("Frühestens (Monate) darf nicht nach Spätestens (Monate) liegen.");
    }
}
