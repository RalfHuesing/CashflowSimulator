using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="EconomicFactorDto"/>.
/// Sinnvolle Ranges für Volatilität, Drift und Mean-Reversion-Speed.
/// </summary>
public sealed class EconomicFactorDtoValidator : AbstractValidator<EconomicFactorDto>
{
    private const int NameMaxLength = 200;
    private const int IdMaxLength = 100;
    private const double ExpectedReturnMin = -0.5;
    private const double ExpectedReturnMax = 0.5;
    private const double VolatilityMin = 0.001;
    private const double VolatilityMax = 2.0;
    private const double MeanReversionSpeedMin = 0;
    private const double MeanReversionSpeedMax = 10;
    private const double InitialValueMin = 1e-6;
    private const double InitialValueMax = 1e6;

    public EconomicFactorDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID des Faktors muss angegeben werden.")
            .MaximumLength(IdMaxLength)
            .WithMessage($"ID darf maximal {IdMaxLength} Zeichen haben.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name des Faktors muss angegeben werden.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Name darf nicht nur aus Leerzeichen bestehen.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name darf maximal {NameMaxLength} Zeichen haben.");

        RuleFor(x => x.ExpectedReturn)
            .InclusiveBetween(ExpectedReturnMin, ExpectedReturnMax)
            .WithMessage($"Erwartete Rendite/Drift muss zwischen {ExpectedReturnMin} und {ExpectedReturnMax} liegen (z. B. 0,07 = 7 %).");

        RuleFor(x => x.Volatility)
            .InclusiveBetween(VolatilityMin, VolatilityMax)
            .WithMessage($"Volatilität muss zwischen {VolatilityMin} und {VolatilityMax} liegen (z. B. 0,15 = 15 %).");

        RuleFor(x => x.MeanReversionSpeed)
            .InclusiveBetween(MeanReversionSpeedMin, MeanReversionSpeedMax)
            .WithMessage($"Mean-Reversion-Speed muss zwischen {MeanReversionSpeedMin} und {MeanReversionSpeedMax} liegen.");

        RuleFor(x => x.InitialValue)
            .GreaterThan(0)
            .WithMessage("Initialwert muss größer als 0 sein.")
            .Must(v => v >= InitialValueMin && v <= InitialValueMax)
            .WithMessage($"Initialwert muss zwischen {InitialValueMin} und {InitialValueMax} liegen.");
    }
}
