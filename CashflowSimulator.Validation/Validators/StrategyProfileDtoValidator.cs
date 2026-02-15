using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="StrategyProfileDto"/>.
/// Pflichtfelder und Wertebereiche: Monate >= 0, Rebalancing-Schwelle 0–1.
/// </summary>
public sealed class StrategyProfileDtoValidator : AbstractValidator<StrategyProfileDto>
{
    private const int NameMaxLength = 200;
    private const int IdMaxLength = 100;
    private const decimal ThresholdMin = 0;
    private const decimal ThresholdMax = 1;
    private const int MaxMonths = 600; // ~50 Jahre

    public StrategyProfileDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID des Strategie-Profils muss angegeben werden.")
            .MaximumLength(IdMaxLength)
            .WithMessage($"ID darf maximal {IdMaxLength} Zeichen haben.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name des Strategie-Profils muss angegeben werden.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Name darf nicht nur aus Leerzeichen bestehen.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name darf maximal {NameMaxLength} Zeichen haben.");

        RuleFor(x => x.CashReserveMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Liquiditätsreserve (Monate) darf nicht negativ sein.")
            .LessThanOrEqualTo(MaxMonths)
            .WithMessage($"Liquiditätsreserve darf maximal {MaxMonths} Monate betragen.");

        RuleFor(x => x.RebalancingThreshold)
            .InclusiveBetween(ThresholdMin, ThresholdMax)
            .WithMessage($"Rebalancing-Schwelle muss zwischen {ThresholdMin} und {ThresholdMax} liegen (z. B. 0,05).");

        RuleFor(x => x.MinimumTransactionAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Mindest-Transaktionsgröße darf nicht negativ sein.");

        RuleFor(x => x.LookaheadMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Lookahead (Monate) darf nicht negativ sein.")
            .LessThanOrEqualTo(MaxMonths)
            .WithMessage($"Lookahead darf maximal {MaxMonths} Monate betragen.");
    }
}
