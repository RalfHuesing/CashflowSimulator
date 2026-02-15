using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="CorrelationEntryDto"/>.
/// Korrelation muss in [-1, 1] liegen; FactorIdA/B werden im Projekt-Kontext geprüft.
/// </summary>
public sealed class CorrelationEntryDtoValidator : AbstractValidator<CorrelationEntryDto>
{
    public CorrelationEntryDtoValidator()
    {
        RuleFor(x => x.FactorIdA)
            .NotEmpty()
            .WithMessage("Faktor A muss ausgewählt werden.");

        RuleFor(x => x.FactorIdB)
            .NotEmpty()
            .WithMessage("Faktor B muss ausgewählt werden.");

        RuleFor(x => x)
            .Must(dto => !string.Equals(dto.FactorIdA, dto.FactorIdB, StringComparison.Ordinal))
            .WithMessage("Faktor A und Faktor B müssen unterschiedlich sein.");

        RuleFor(x => x.Correlation)
            .InclusiveBetween(-1.0, 1.0)
            .WithMessage("Korrelation muss zwischen -1 und 1 liegen.");
    }
}
