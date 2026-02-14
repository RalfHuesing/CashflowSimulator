using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="SimulationParametersDto"/>.
/// Fachliche Regeln: Renteneintritt nach Geburt, Lebenserwartung nach Renteneintritt, sinnvolle Altersgrenzen.
/// </summary>
public sealed class SimulationParametersValidator : AbstractValidator<SimulationParametersDto>
{
    private const int MinRetirementAge = 50;
    private const int MaxRetirementAge = 75;
    private const int MinLifeExpectancy = 60;
    private const int MaxLifeExpectancy = 120;

    public SimulationParametersValidator()
    {
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Geburtsdatum muss angegeben werden.");

        RuleFor(x => x.RetirementDate)
            .NotEmpty()
            .WithMessage("Renteneintrittsdatum muss angegeben werden.")
            .Must((dto, retirement) => retirement > dto.DateOfBirth)
            .When(x => x.DateOfBirth != default && x.RetirementDate != default)
            .WithMessage("Renteneintritt muss nach dem Geburtsdatum liegen.");

        RuleFor(x => x)
            .Must(dto => GetAgeInYears(dto.DateOfBirth, dto.RetirementDate) >= MinRetirementAge &&
                         GetAgeInYears(dto.DateOfBirth, dto.RetirementDate) <= MaxRetirementAge)
            .When(x => x.DateOfBirth != default && x.RetirementDate != default)
            .WithMessage($"Renteneintrittsalter muss zwischen {MinRetirementAge} und {MaxRetirementAge} Jahren liegen.");

        RuleFor(x => x.SimulationEnd)
            .NotEmpty()
            .WithMessage("Simulationsende (Lebenserwartung) muss angegeben werden.")
            .Must((dto, end) => end > dto.RetirementDate)
            .When(x => x.RetirementDate != default && x.SimulationEnd != default)
            .WithMessage("Lebenserwartung muss nach dem Renteneintritt liegen.");

        RuleFor(x => x)
            .Must(dto => GetAgeInYears(dto.DateOfBirth, dto.SimulationEnd) >= MinLifeExpectancy &&
                         GetAgeInYears(dto.DateOfBirth, dto.SimulationEnd) <= MaxLifeExpectancy)
            .When(x => x.DateOfBirth != default && x.SimulationEnd != default)
            .WithMessage($"Lebenserwartung muss zwischen {MinLifeExpectancy} und {MaxLifeExpectancy} Jahren liegen.");

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
