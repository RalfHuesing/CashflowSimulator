using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Eckdaten;

/// <summary>
/// ViewModel für die Eckdaten-Seite (Simulationsparameter).
/// UI zeigt menschliche Eingaben (Geburtsdatum, Rentenalter, Lebenserwartung);
/// Transformation in <see cref="SimulationParametersDto"/> für die Engine erfolgt hier.
/// Validierung über <see cref="ValidationRunner"/>; Fehler inline am Control (INotifyDataErrorInfo).
/// Auto-Save bei jeder Änderung (auch bei ungültigen Daten).
/// </summary>
public partial class EckdatenViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private static readonly IReadOnlyDictionary<string, string> DtoToVmPropertyMap = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        { nameof(SimulationParametersDto.DateOfBirth), nameof(DateOfBirth) },
        { nameof(SimulationParametersDto.SimulationEnd), nameof(LifeExpectancy) },
        { nameof(SimulationParametersDto.InitialLossCarryforwardGeneral), nameof(InitialLossCarryforwardGeneral) },
        { nameof(SimulationParametersDto.InitialLossCarryforwardStocks), nameof(InitialLossCarryforwardStocks) },
        { nameof(SimulationParametersDto.InitialLiquidCash), nameof(InitialLiquidCash) },
        { nameof(SimulationParametersDto.CurrencyCode), nameof(InitialLiquidCash) }
    };

    /// <inheritdoc />
    protected override string HelpKeyPrefix => "Eckdaten";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentAgeText))]
    private DateOnly? _dateOfBirth;

    [ObservableProperty]
    private decimal _lifeExpectancy = 95;

    [ObservableProperty]
    private decimal _initialLossCarryforwardGeneral;

    [ObservableProperty]
    private decimal _initialLossCarryforwardStocks;

    [ObservableProperty]
    private decimal _initialLiquidCash;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    public EckdatenViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "Eckdaten";
        LoadFromProject();
        ValidateAndSave();
    }

    /// <summary>Read-only: aktuelles Alter in Jahren (zur Kontrolle).</summary>
    public string CurrentAgeText => DateOfBirth.HasValue
        ? $"{GetAgeInYears(DateOfBirth.Value)} Jahre"
        : "—";

    partial void OnDateOfBirthChanged(DateOnly? value) => ScheduleValidateAndSave();
    partial void OnLifeExpectancyChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnInitialLossCarryforwardGeneralChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnInitialLossCarryforwardStocksChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnInitialLiquidCashChanged(decimal value) => ScheduleValidateAndSave();

    private void ScheduleValidateAndSave()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateAndSave);
    }

    private void LoadFromProject()
    {
        var p = _currentProjectService.Current?.Parameters;
        if (p is null)
            return;

        _isLoading = true;
        try
        {
            DateOfBirth = p.DateOfBirth;
            LifeExpectancy = p.SimulationEnd.Year - p.DateOfBirth.Year;
            InitialLossCarryforwardGeneral = p.InitialLossCarryforwardGeneral;
            InitialLossCarryforwardStocks = p.InitialLossCarryforwardStocks;
            InitialLiquidCash = p.InitialLiquidCash;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static int GetAgeInYears(DateOnly birth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birth.Year;
        if (birth > today.AddYears(-age))
            age--;
        return age;
    }

    private void ValidateAndSave()
    {
        var dto = BuildDto();
        if (dto is null)
        {
            SetValidationErrors([new ValidationError(nameof(SimulationParametersDto.DateOfBirth), "Geburtsdatum muss angegeben werden.")], DtoToVmPropertyMap);
            return;
        }

        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors, DtoToVmPropertyMap);
        _currentProjectService.UpdateParameters(dto);
    }

    private SimulationParametersDto? BuildDto()
    {
        if (!DateOfBirth.HasValue)
            return null;

        var dob = DateOfBirth.Value;
        var simulationStart = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
        var lifeYears = (int)Math.Round(LifeExpectancy);
        var simulationEnd = FirstOfMonthWhenAge(dob, lifeYears);

        return new SimulationParametersDto
        {
            SimulationStart = simulationStart,
            SimulationEnd = simulationEnd,
            DateOfBirth = dob,
            InitialLossCarryforwardGeneral = InitialLossCarryforwardGeneral,
            InitialLossCarryforwardStocks = InitialLossCarryforwardStocks,
            InitialLiquidCash = InitialLiquidCash,
            CurrencyCode = "EUR"
        };
    }

    private static DateOnly FirstOfMonthWhenAge(DateOnly dateOfBirth, int age)
    {
        var atAge = dateOfBirth.AddYears(age);
        return new DateOnly(atAge.Year, atAge.Month, 1);
    }
}
