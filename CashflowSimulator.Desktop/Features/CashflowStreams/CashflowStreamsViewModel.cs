using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.CashflowStreams;

/// <summary>
/// ViewModel für laufende Cashflows (Streams), gefiltert nach Einnahmen oder Ausgaben.
/// Validierung über <see cref="ValidationRunner"/>; Fehler nur im Info-Panel.
/// Property-Namen 1:1 wie im DTO (Rules-konform).
/// </summary>
public partial class CashflowStreamsViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private readonly ICurrentProjectService _currentProjectService;
    private readonly CashflowType _cashflowType;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private CashflowStreamDto? _selectedItem;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _interval = "Monthly";

    [ObservableProperty]
    private DateOnly? _startDate;

    [ObservableProperty]
    private DateOnly? _endDate;

    /// <summary>Optional: Marktfaktor zur Dynamisierung (null = Keine).</summary>
    [ObservableProperty]
    private string? _economicFactorId;

    /// <summary>Für ComboBox: „Keine“ + alle Marktfaktoren.</summary>
    [ObservableProperty]
    private DynamicFactorOption? _selectedDynamicFactor;

    partial void OnSelectedDynamicFactorChanged(DynamicFactorOption? value)
    {
        _economicFactorId = value?.Id;
        ScheduleValidateAndSave();
    }

    /// <summary>Id des Eintrags im Bearbeitungsformular (null = Neu).</summary>
    [ObservableProperty]
    private string? _editingId;

    public ObservableCollection<CashflowStreamDto> Items { get; } = [];

    public ObservableCollection<DynamicFactorOption> DynamicFactorOptions { get; } = [];

    public string Title => _cashflowType == CashflowType.Income ? "Laufende Einnahmen" : "Laufende Ausgaben";

    public static IReadOnlyList<string> IntervalOptions { get; } = ["Monthly", "Yearly"];

    /// <inheritdoc />
    protected override string HelpKeyPrefix => "CashflowStreams";

    public CashflowStreamsViewModel(
        ICurrentProjectService currentProjectService,
        IHelpProvider helpProvider,
        CashflowType cashflowType)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        _cashflowType = cashflowType;
        PageHelpKey = "CashflowStreams";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshDynamicFactorOptions();
        RefreshItems();
    }

    partial void OnSelectedItemChanged(CashflowStreamDto? value)
    {
        if (value is null)
        {
            ClearForm();
            return;
        }
        _isLoading = true;
        try
        {
            EditingId = value.Id;
            Name = value.Name;
            Amount = value.Amount;
            Interval = value.Interval;
            StartDate = value.StartDate;
            EndDate = value.EndDate;
            EconomicFactorId = value.EconomicFactorId;
            SelectedDynamicFactor = DynamicFactorOptions.FirstOrDefault(o => o.Id == value.EconomicFactorId);
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnAmountChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnIntervalChanged(string value) => ScheduleValidateAndSave();
    partial void OnStartDateChanged(DateOnly? value) => ScheduleValidateAndSave();
    partial void OnEndDateChanged(DateOnly? value) => ScheduleValidateAndSave();

    private void ScheduleValidateAndSave()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateAndSave);
    }

    private void ValidateAndSave()
    {
        var dto = BuildStreamDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
    }

    private CashflowStreamDto BuildStreamDtoFromForm()
    {
        return new CashflowStreamDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            Type = _cashflowType,
            Amount = Amount,
            Interval = Interval,
            StartDate = StartDate.GetValueOrDefault(),
            EndDate = EndDate,
            EconomicFactorId = EconomicFactorId
        };
    }

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshDynamicFactorOptions();
        RefreshItems();
    }

    private void RefreshDynamicFactorOptions()
    {
        DynamicFactorOptions.Clear();
        DynamicFactorOptions.Add(new DynamicFactorOption(null, "Keine"));
        var factors = _currentProjectService.Current?.EconomicFactors;
        if (factors is not null)
        {
            foreach (var f in factors)
                DynamicFactorOptions.Add(new DynamicFactorOption(f.Id, f.Name));
        }
    }

    private void RefreshItems()
    {
        var current = _currentProjectService.Current;
        Items.Clear();
        if (current?.Streams is null)
            return;
        foreach (var item in current.Streams.Where(s => s.Type == _cashflowType))
            Items.Add(item);
    }

    private void ClearForm()
    {
        EditingId = null;
        Name = string.Empty;
        Amount = 0;
        Interval = "Monthly";
        var start = _currentProjectService.Current?.Parameters.SimulationStart ?? DateOnly.FromDateTime(DateTime.Today);
        StartDate = start;
        EndDate = null;
        EconomicFactorId = null;
        SelectedDynamicFactor = DynamicFactorOptions.FirstOrDefault(o => o.Id is null);
        ClearValidationErrors();
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        var start = _currentProjectService.Current?.Parameters.SimulationStart ?? DateOnly.FromDateTime(DateTime.Today);
        StartDate = start;
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildStreamDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
        if (!validationResult.IsValid)
            return;

        var list = current.Streams.ToList();
        if (EditingId is null)
        {
            var newItem = new CashflowStreamDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name.Trim(),
                Type = _cashflowType,
                Amount = Amount,
                Interval = Interval,
                StartDate = StartDate!.Value,
                EndDate = EndDate,
                EconomicFactorId = EconomicFactorId
            };
            list.Add(newItem);
            _currentProjectService.UpdateStreams(list);
            RefreshItems();
            SelectedItem = Items.FirstOrDefault(x => x.Id == newItem.Id);
            ClearForm();
        }
        else
        {
            var idx = list.FindIndex(x => x.Id == EditingId);
            if (idx < 0)
                return;
            list[idx] = list[idx] with
            {
                Name = Name.Trim(),
                Amount = Amount,
                Interval = Interval,
                StartDate = StartDate!.Value,
                EndDate = EndDate,
                EconomicFactorId = EconomicFactorId
            };
            _currentProjectService.UpdateStreams(list);
            RefreshItems();
            SelectedItem = Items.FirstOrDefault(x => x.Id == EditingId);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete()
    {
        if (SelectedItem is null)
            return;
        var current = _currentProjectService.Current;
        if (current is null)
            return;
        var list = current.Streams.Where(x => x.Id != SelectedItem.Id).ToList();
        _currentProjectService.UpdateStreams(list);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}

/// <summary>Eintrag für Dynamisierung-ComboBox (Id null = „Keine“).</summary>
public record DynamicFactorOption(string? Id, string Display);
