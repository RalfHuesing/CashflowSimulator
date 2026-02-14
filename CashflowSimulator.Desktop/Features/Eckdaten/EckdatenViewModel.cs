using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Eckdaten;

/// <summary>
/// ViewModel für die Eckdaten-Seite (Simulationsparameter).
/// UI zeigt menschliche Eingaben (Geburtsdatum, Rentenalter, Lebenserwartung);
/// Transformation in <see cref="SimulationParametersDto"/> für die Engine erfolgt hier.
/// Validierung ausschließlich über <see cref="ValidationRunner"/>; Fehler im zentralen Meldungsbereich.
/// </summary>
public partial class EckdatenViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentAgeText))]
    private DateTimeOffset? _birthDate;

    [ObservableProperty]
    private decimal _retirementAge = 67;

    [ObservableProperty]
    private decimal _lifeExpectancy = 95;

    [ObservableProperty]
    private decimal _initialLiquidCash;

    private readonly ICurrentProjectService _currentProjectService;
    private readonly IValidationMessageService _validationMessageService;

    public EckdatenViewModel(ICurrentProjectService currentProjectService, IValidationMessageService validationMessageService)
    {
        _currentProjectService = currentProjectService;
        _validationMessageService = validationMessageService;
        LoadFromProject();
    }

    /// <summary>Read-only: aktuelles Alter in Jahren (zur Kontrolle).</summary>
    public string CurrentAgeText => BirthDate.HasValue
        ? $"{GetAgeInYears(BirthDate.Value)} Jahre"
        : "—";

    private void LoadFromProject()
    {
        var p = _currentProjectService.Current?.Parameters;
        if (p is null)
            return;

        BirthDate = new DateTimeOffset(p.DateOfBirth.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        RetirementAge = p.RetirementDate.Year - p.DateOfBirth.Year;
        LifeExpectancy = p.SimulationEnd.Year - p.DateOfBirth.Year;
        InitialLiquidCash = p.InitialLiquidCash;
    }

    private static int GetAgeInYears(DateTimeOffset birth)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.Date);
        var birthDate = DateOnly.FromDateTime(birth.Date);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
            age--;
        return age;
    }

    [RelayCommand]
    private void Discard()
    {
        LoadFromProject();
    }

    [RelayCommand]
    private void Apply()
    {
        if (!BirthDate.HasValue)
        {
            _validationMessageService.SetErrors("Eckdaten", [new Contracts.Common.ValidationError("DateOfBirth", "Geburtsdatum muss angegeben werden.")]);
            return;
        }

        var dob = DateOnly.FromDateTime(BirthDate.Value.Date);
        var simulationStart = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
        var retirementYears = (int)Math.Round(RetirementAge);
        var lifeYears = (int)Math.Round(LifeExpectancy);
        var retirementDate = FirstOfMonthWhenAge(dob, retirementYears);
        var simulationEnd = FirstOfMonthWhenAge(dob, lifeYears);

        var dto = new SimulationParametersDto
        {
            SimulationStart = simulationStart,
            SimulationEnd = simulationEnd,
            DateOfBirth = dob,
            RetirementDate = retirementDate,
            InitialLiquidCash = InitialLiquidCash,
            CurrencyCode = "EUR"
        };

        var validationResult = ValidationRunner.Validate(dto);
        if (!validationResult.IsValid)
        {
            _validationMessageService.SetErrors("Eckdaten", validationResult.Errors);
            return;
        }

        _validationMessageService.ClearSource("Eckdaten");
        _currentProjectService.UpdateParameters(dto);
    }

    private static DateOnly FirstOfMonthWhenAge(DateOnly dateOfBirth, int age)
    {
        var atAge = dateOfBirth.AddYears(age);
        return new DateOnly(atAge.Year, atAge.Month, 1);
    }
}
