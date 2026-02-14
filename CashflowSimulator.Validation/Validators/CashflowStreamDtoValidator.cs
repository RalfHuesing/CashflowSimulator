using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="CashflowStreamDto"/>.
/// </summary>
public sealed class CashflowStreamDtoValidator : AbstractValidator<CashflowStreamDto>
{
    private const int NameMaxLength = 200;
    private const int DateMinYear = 1900;
    private const int DateMaxYear = 2100;

    private static readonly string[] AllowedIntervals = ["Monthly", "Yearly"];

    public CashflowStreamDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name muss angegeben werden.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Name darf nicht nur aus Leerzeichen bestehen.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name darf maximal {NameMaxLength} Zeichen haben.");

        RuleFor(x => x.StartDate)
            .Must(d => d != default)
            .WithMessage("Startdatum muss angegeben werden.");
        RuleFor(x => x.StartDate)
            .Must(d => d.Year >= DateMinYear && d.Year <= DateMaxYear)
            .WithMessage($"Startdatum muss im Bereich {DateMinYear}–{DateMaxYear} liegen.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Betrag muss größer als 0 sein.");

        RuleFor(x => x.Interval)
            .NotEmpty()
            .WithMessage("Intervall muss angegeben werden.")
            .Must(interval => AllowedIntervals.Contains(interval))
            .WithMessage("Intervall muss 'Monthly' oder 'Yearly' sein.");

        RuleFor(x => x.EndDate)
            .Must(d => !d.HasValue || (d.Value.Year >= DateMinYear && d.Value.Year <= DateMaxYear))
            .When(x => x.EndDate.HasValue)
            .WithMessage($"Enddatum muss im Bereich {DateMinYear}–{DateMaxYear} liegen.");

        RuleFor(x => x)
            .Must(dto => !dto.EndDate.HasValue || dto.EndDate >= dto.StartDate)
            .When(x => x.StartDate != default && x.EndDate.HasValue)
            .WithMessage("Enddatum darf nicht vor dem Startdatum liegen.");
    }
}
