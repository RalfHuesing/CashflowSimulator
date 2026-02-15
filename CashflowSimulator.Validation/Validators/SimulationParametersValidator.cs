using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="SimulationParametersDto"/>.
/// Fachliche Regeln: Lebenserwartung nach Geburt, sinnvolle Altersgrenzen, Verlustvorträge und Start-Kapital nicht negativ.
/// </summary>
public sealed class SimulationParametersValidator : AbstractValidator<SimulationParametersDto>
{
    private const int MinLifeExpectancy = 60;
    private const int MaxLifeExpectancy = 120;

    public SimulationParametersValidator()
    {
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Geburtsdatum muss angegeben werden.");

        RuleFor(x => x.SimulationEnd)
            .NotEmpty()
            .WithMessage("Simulationsende (Lebenserwartung) muss angegeben werden.")
            .Must((dto, end) => end > dto.DateOfBirth)
            .When(x => x.DateOfBirth != default && x.SimulationEnd != default)
            .WithMessage("Lebenserwartung muss nach dem Geburtsdatum liegen.");

        RuleFor(x => x)
            .Must(dto => GetAgeInYears(dto.DateOfBirth, dto.SimulationEnd) >= MinLifeExpectancy &&
                         GetAgeInYears(dto.DateOfBirth, dto.SimulationEnd) <= MaxLifeExpectancy)
            .When(x => x.DateOfBirth != default && x.SimulationEnd != default)
            .WithMessage($"Lebenserwartung muss zwischen {MinLifeExpectancy} und {MaxLifeExpectancy} Jahren liegen.");

        RuleFor(x => x.InitialLossCarryforwardGeneral)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Verlustvortrag (allgemein) darf nicht negativ sein.");

        RuleFor(x => x.InitialLossCarryforwardStocks)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Verlustvortrag (Aktien) darf nicht negativ sein.");

        RuleFor(x => x.InitialLiquidCash)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Start-Kapital darf nicht negativ sein.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Währung muss angegeben werden.");
    }

    private static int GetAgeInYears(DateOnly from, DateOnly to)
    {
        var years = to.Year - from.Year;
        if (from.AddYears(years) > to)
            years--;
        return years;
    }
}
