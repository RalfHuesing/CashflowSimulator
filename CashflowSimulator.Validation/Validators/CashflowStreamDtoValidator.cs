using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="CashflowStreamDto"/>.
/// </summary>
public sealed class CashflowStreamDtoValidator : AbstractValidator<CashflowStreamDto>
{
    private static readonly string[] AllowedIntervals = ["Monthly", "Yearly"];

    public CashflowStreamDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name muss angegeben werden.");

        RuleFor(x => x.StartDate)
            .Must(d => d != default)
            .WithMessage("Startdatum muss angegeben werden.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Betrag darf nicht negativ sein.");

        RuleFor(x => x.Interval)
            .Must(interval => AllowedIntervals.Contains(interval))
            .WithMessage("Intervall muss 'Monthly' oder 'Yearly' sein.");

        RuleFor(x => x)
            .Must(dto => !dto.EndDate.HasValue || dto.EndDate >= dto.StartDate)
            .When(x => x.StartDate != default && x.EndDate.HasValue)
            .WithMessage("Enddatum darf nicht vor dem Startdatum liegen.");
    }
}
