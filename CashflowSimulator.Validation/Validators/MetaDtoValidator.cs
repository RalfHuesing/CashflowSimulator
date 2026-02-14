using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="MetaDto"/>.
/// </summary>
public sealed class MetaDtoValidator : AbstractValidator<MetaDto>
{
    private const int ScenarioNameMaxLength = 200;

    public MetaDtoValidator()
    {
        RuleFor(x => x.ScenarioName)
            .NotEmpty()
            .WithMessage("Szenario-Name darf nicht leer sein.")
            .MaximumLength(ScenarioNameMaxLength)
            .WithMessage($"Szenario-Name darf maximal {ScenarioNameMaxLength} Zeichen haben.");
    }
}
