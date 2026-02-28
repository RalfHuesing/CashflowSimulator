using CashflowSimulator.Contracts.Dtos;
using FluentValidation;

namespace CashflowSimulator.Validation.Validators;

/// <summary>
/// FluentValidation-Validator für <see cref="AssetDto"/>.
/// Validiert u.a. die Tranchen (FIFO-Bestand).
/// </summary>
public sealed class AssetDtoValidator : AbstractValidator<AssetDto>
{
    public AssetDtoValidator()
    {
        RuleForEach(x => x.Tranches)
            .SetValidator(new AssetTrancheDtoValidator());
    }
}
