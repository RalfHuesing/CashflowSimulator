using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation.Validators;
using FluentValidation;

namespace CashflowSimulator.Validation;

/// <summary>
/// Zentrale Stelle zum Ausführen der Validatoren.
/// Mappt FluentValidation-Ergebnisse auf <see cref="Contracts.Common.ValidationResult"/>.
/// </summary>
public static class ValidationRunner
{
    private static readonly SimulationParametersValidator ParametersValidator = new();
    private static readonly MetaDtoValidator MetaValidator = new();
    private static readonly SimulationProjectValidator ProjectValidator = new();
    private static readonly CashflowStreamDtoValidator StreamValidator = new();
    private static readonly CashflowEventDtoValidator EventValidator = new();
    private static readonly EconomicFactorDtoValidator EconomicFactorValidator = new();
    private static readonly CorrelationEntryDtoValidator CorrelationEntryValidator = new();

    /// <summary>
    /// Validiert <see cref="SimulationParametersDto"/> (z. B. für Eckdaten-Apply).
    /// </summary>
    public static ValidationResult Validate(SimulationParametersDto dto)
    {
        var result = ParametersValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="MetaDto"/> (z. B. für Szenario-Meta-Apply).
    /// </summary>
    public static ValidationResult Validate(MetaDto dto)
    {
        var result = MetaValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert das komplette <see cref="SimulationProjectDto"/> (z. B. vor Simulation oder nach Laden).
    /// </summary>
    public static ValidationResult Validate(SimulationProjectDto dto)
    {
        var result = ProjectValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="CashflowStreamDto"/> (z. B. für das Bearbeitungsformular Laufende Cashflows).
    /// </summary>
    public static ValidationResult Validate(CashflowStreamDto dto)
    {
        var result = StreamValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="CashflowEventDto"/> (z. B. für das Bearbeitungsformular Geplante Cashflows).
    /// </summary>
    public static ValidationResult Validate(CashflowEventDto dto)
    {
        var result = EventValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="EconomicFactorDto"/> (z. B. für das Marktdaten-Formular).
    /// </summary>
    public static ValidationResult Validate(EconomicFactorDto dto)
    {
        var result = EconomicFactorValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="CorrelationEntryDto"/> (z. B. für das Korrelationen-Formular).
    /// </summary>
    public static ValidationResult Validate(CorrelationEntryDto dto)
    {
        var result = CorrelationEntryValidator.Validate(dto);
        return ToValidationResult(result);
    }

    private static ValidationResult ToValidationResult(FluentValidation.Results.ValidationResult result)
    {
        if (result.IsValid)
            return ValidationResult.Success();

        var errors = result.Errors
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();
        return ValidationResult.Failure(errors);
    }
}
