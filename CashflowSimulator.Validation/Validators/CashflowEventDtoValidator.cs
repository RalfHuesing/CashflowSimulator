using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="CashflowEventDto"/>.
/// </summary>
public sealed class CashflowEventDtoValidator : AbstractValidator<CashflowEventDto>
{
    public CashflowEventDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name muss angegeben werden.");

        RuleFor(x => x.TargetDate)
            .Must(d => d != default)
            .WithMessage("Zieldatum muss angegeben werden.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Betrag darf nicht negativ sein.");

        RuleFor(x => x.EarliestMonthOffset)
            .InclusiveBetween(-120, 0)
            .When(x => x.EarliestMonthOffset.HasValue)
            .WithMessage("Frühestens (Monate) muss zwischen -120 und 0 liegen.");

        RuleFor(x => x.LatestMonthOffset)
            .InclusiveBetween(0, 120)
            .When(x => x.LatestMonthOffset.HasValue)
            .WithMessage("Spätestens (Monate) muss zwischen 0 und 120 liegen.");

        RuleFor(x => x)
            .Must(dto => !dto.EarliestMonthOffset.HasValue || !dto.LatestMonthOffset.HasValue ||
                         dto.EarliestMonthOffset <= dto.LatestMonthOffset)
            .WithMessage("Frühestens (Monate) darf nicht nach Spätestens (Monate) liegen.");
    }
}
