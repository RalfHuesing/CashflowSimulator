using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="TaxProfileDto"/>.
/// Pflichtfelder und Wertebereiche für Steuersätze (0–1), Freibetrag >= 0.
/// </summary>
public sealed class TaxProfileDtoValidator : AbstractValidator<TaxProfileDto>
{
    private const int NameMaxLength = 200;
    private const int IdMaxLength = 100;
    private const decimal RateMin = 0;
    private const decimal RateMax = 1;

    public TaxProfileDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID des Steuer-Profils muss angegeben werden.")
            .MaximumLength(IdMaxLength)
            .WithMessage($"ID darf maximal {IdMaxLength} Zeichen haben.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name des Steuer-Profils muss angegeben werden.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Name darf nicht nur aus Leerzeichen bestehen.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name darf maximal {NameMaxLength} Zeichen haben.");

        RuleFor(x => x.CapitalGainsTaxRate)
            .InclusiveBetween(RateMin, RateMax)
            .WithMessage($"Kapitalertragsteuer-Satz muss zwischen {RateMin} und {RateMax} liegen (z. B. 0,26375).");

        RuleFor(x => x.TaxFreeAllowance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Freibetrag darf nicht negativ sein.");

        RuleFor(x => x.IncomeTaxRate)
            .InclusiveBetween(RateMin, RateMax)
            .WithMessage($"Einkommensteuer-Satz muss zwischen {RateMin} und {RateMax} liegen.");
    }
}
