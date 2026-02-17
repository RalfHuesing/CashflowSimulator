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
    private static readonly TaxProfileDtoValidator TaxProfileValidator = new();
    private static readonly StrategyProfileDtoValidator StrategyProfileValidator = new();
    private static readonly AllocationProfileDtoValidator AllocationProfileValidator = new();
    private static readonly LifecyclePhaseDtoValidator LifecyclePhaseValidator = new();

    /// <summary>
    /// Validiert <see cref="SimulationParametersDto"/> (z. B. für Eckdaten-Apply).
    /// </summary>
    public static ValidationResult Validate(SimulationParametersDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = ParametersValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="MetaDto"/> (z. B. für Szenario-Meta-Apply).
    /// </summary>
    public static ValidationResult Validate(MetaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = MetaValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert das komplette <see cref="SimulationProjectDto"/> (z. B. vor Simulation oder nach Laden).
    /// </summary>
    public static ValidationResult Validate(SimulationProjectDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = ProjectValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="CashflowStreamDto"/> (z. B. für das Bearbeitungsformular Laufende Cashflows).
    /// </summary>
    public static ValidationResult Validate(CashflowStreamDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = StreamValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="CashflowEventDto"/> (z. B. für das Bearbeitungsformular Geplante Cashflows).
    /// </summary>
    public static ValidationResult Validate(CashflowEventDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = EventValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="EconomicFactorDto"/> (z. B. für das Marktdaten-Formular).
    /// </summary>
    public static ValidationResult Validate(EconomicFactorDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = EconomicFactorValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="CorrelationEntryDto"/> (z. B. für das Korrelationen-Formular).
    /// </summary>
    public static ValidationResult Validate(CorrelationEntryDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = CorrelationEntryValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="TaxProfileDto"/> (z. B. für das Steuer-Profil-Formular).
    /// </summary>
    public static ValidationResult Validate(TaxProfileDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = TaxProfileValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="StrategyProfileDto"/> (z. B. für das Strategie-Profil-Formular).
    /// </summary>
    public static ValidationResult Validate(StrategyProfileDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = StrategyProfileValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="AllocationProfileDto"/> (z. B. für das Allokationsprofil-Formular).
    /// </summary>
    public static ValidationResult Validate(AllocationProfileDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = AllocationProfileValidator.Validate(dto);
        return ToValidationResult(result);
    }

    /// <summary>
    /// Validiert <see cref="LifecyclePhaseDto"/> (z. B. für das Lebensphasen-Formular).
    /// </summary>
    public static ValidationResult Validate(LifecyclePhaseDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var result = LifecyclePhaseValidator.Validate(dto);
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
