using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für das komplette <see cref="SimulationProjectDto"/>.
/// Bündelt Meta- und Parameters-Validierung; wird von der Engine an Einstiegspunkten genutzt.
/// </summary>
public sealed class SimulationProjectValidator : AbstractValidator<SimulationProjectDto>
{
    public SimulationProjectValidator()
    {
        RuleFor(x => x.Meta)
            .NotNull()
            .SetValidator(new MetaDtoValidator());

        RuleFor(x => x.Parameters)
            .NotNull()
            .SetValidator(new SimulationParametersValidator());

        RuleFor(x => x.UiSettings)
            .NotNull();
    }
}
