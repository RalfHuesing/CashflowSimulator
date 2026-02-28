using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="AssetTrancheDto"/> (FIFO-Kauf-Tranche).
/// </summary>
public sealed class AssetTrancheDtoValidator : AbstractValidator<AssetTrancheDto>
{
    public AssetTrancheDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Die Menge einer Tranche muss größer als 0 sein.");

        RuleFor(x => x.AcquisitionPricePerUnit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Der Anschaffungspreis pro Stück darf nicht negativ sein.");
    }
}
