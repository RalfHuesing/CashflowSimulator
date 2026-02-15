using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.CashflowEvents;

/// <summary>
/// ViewModel für geplante Cashflow-Events, gefiltert nach Einnahmen oder Ausgaben.
/// Validierung über <see cref="ValidationRunner"/>; Fehler nur im Info-Panel.
/// Property-Namen 1:1 wie im DTO (Rules-konform).
/// </summary>
public partial class CashflowEventsViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private readonly ICurrentProjectService _currentProjectService;
    private readonly CashflowType _cashflowType;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private CashflowEventDto? _selectedItem;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private DateOnly? _targetDate;

    [ObservableProperty]
    private int? _earliestMonthOffset;

    [ObservableProperty]
    private int? _latestMonthOffset;

    [ObservableProperty]
    private string? _editingId;

    public ObservableCollection<CashflowEventDto> Items { get; } = [];

    public string Title => _cashflowType == CashflowType.Income ? "Geplante Einnahmen" : "Geplante Ausgaben";

    /// <inheritdoc />
    protected override string HelpKeyPrefix => "CashflowEvents";

    public CashflowEventsViewModel(
        ICurrentProjectService currentProjectService,
        IHelpProvider helpProvider,
        CashflowType cashflowType)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        _cashflowType = cashflowType;
        PageHelpKey = "CashflowEvents";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshItems();
    }

    partial void OnSelectedItemChanged(CashflowEventDto? value)
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
            TargetDate = value.TargetDate;
            EarliestMonthOffset = value.EarliestMonthOffset;
            LatestMonthOffset = value.LatestMonthOffset;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnAmountChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnTargetDateChanged(DateOnly? value) => ScheduleValidateAndSave();
    partial void OnEarliestMonthOffsetChanged(int? value) => ScheduleValidateAndSave();
    partial void OnLatestMonthOffsetChanged(int? value) => ScheduleValidateAndSave();

    private void ScheduleValidateAndSave()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateAndSave);
    }

    private void ValidateAndSave()
    {
        var dto = BuildEventDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
    }

    private CashflowEventDto BuildEventDtoFromForm()
    {
        return new CashflowEventDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            Type = _cashflowType,
            Amount = Amount,
            TargetDate = TargetDate.GetValueOrDefault(),
            EarliestMonthOffset = EarliestMonthOffset,
            LatestMonthOffset = LatestMonthOffset
        };
    }

    private void OnProjectChanged(object? sender, EventArgs e) => RefreshItems();

    private void RefreshItems()
    {
        var current = _currentProjectService.Current;
        Items.Clear();
        if (current?.Events is null)
            return;
        foreach (var item in current.Events.Where(e => e.Type == _cashflowType))
            Items.Add(item);
    }

    private void ClearForm()
    {
        EditingId = null;
        Name = string.Empty;
        Amount = 0;
        TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
        EarliestMonthOffset = null;
        LatestMonthOffset = null;
        ClearValidationErrors();
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildEventDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
        if (!validationResult.IsValid)
            return;

        var list = current.Events.ToList();
        if (EditingId is null)
        {
            var newItem = new CashflowEventDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name.Trim(),
                Type = _cashflowType,
                Amount = Amount,
                TargetDate = TargetDate!.Value,
                EarliestMonthOffset = EarliestMonthOffset,
                LatestMonthOffset = LatestMonthOffset
            };
            list.Add(newItem);
            _currentProjectService.UpdateEvents(list);
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
                TargetDate = TargetDate!.Value,
                EarliestMonthOffset = EarliestMonthOffset,
                LatestMonthOffset = LatestMonthOffset
            };
            _currentProjectService.UpdateEvents(list);
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
        var list = current.Events.Where(x => x.Id != SelectedItem.Id).ToList();
        _currentProjectService.UpdateEvents(list);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}
