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
    private DateTimeOffset? _startDate;

    [ObservableProperty]
    private DateTimeOffset? _endDate;

    /// <summary>Id des Eintrags im Bearbeitungsformular (null = Neu).</summary>
    [ObservableProperty]
    private string? _editingId;

    public ObservableCollection<CashflowStreamDto> Items { get; } = [];

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
            StartDate = new DateTimeOffset(value.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            EndDate = value.EndDate.HasValue ? new DateTimeOffset(value.EndDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : null;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnAmountChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnIntervalChanged(string value) => ScheduleValidateAndSave();
    partial void OnStartDateChanged(DateTimeOffset? value) => ScheduleValidateAndSave();
    partial void OnEndDateChanged(DateTimeOffset? value) => ScheduleValidateAndSave();

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
        var startDateValue = StartDate.HasValue ? DateOnly.FromDateTime(StartDate.Value.Date) : default;
        var endDateValue = EndDate.HasValue ? (DateOnly?)DateOnly.FromDateTime(EndDate.Value.Date) : null;
        return new CashflowStreamDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            Type = _cashflowType,
            Amount = Amount,
            Interval = Interval,
            StartDate = startDateValue,
            EndDate = endDateValue
        };
    }

    private void OnProjectChanged(object? sender, EventArgs e) => RefreshItems();

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
        StartDate = new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        EndDate = null;
        ClearValidationErrors();
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        var start = _currentProjectService.Current?.Parameters.SimulationStart ?? DateOnly.FromDateTime(DateTime.Today);
        StartDate = new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
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

        var startDateValue = DateOnly.FromDateTime(StartDate!.Value.Date);
        var endDateValue = EndDate.HasValue ? (DateOnly?)DateOnly.FromDateTime(EndDate.Value.Date) : null;
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
                StartDate = startDateValue,
                EndDate = endDateValue
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
                StartDate = startDateValue,
                EndDate = endDateValue
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
