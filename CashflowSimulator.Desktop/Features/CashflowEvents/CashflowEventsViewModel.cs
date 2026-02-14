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
/// </summary>
public partial class CashflowEventsViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private static readonly IReadOnlyDictionary<string, string> DtoToVmPropertyMap = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        { nameof(CashflowEventDto.Name), nameof(EditName) },
        { nameof(CashflowEventDto.Amount), nameof(EditAmount) },
        { nameof(CashflowEventDto.TargetDate), nameof(EditTargetDate) },
        { nameof(CashflowEventDto.EarliestMonthOffset), nameof(EditEarliestMonthOffset) },
        { nameof(CashflowEventDto.LatestMonthOffset), nameof(EditLatestMonthOffset) }
    };

    private readonly ICurrentProjectService _currentProjectService;
    private readonly CashflowType _cashflowType;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private CashflowEventDto? _selectedItem;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private decimal _editAmount;

    [ObservableProperty]
    private DateTimeOffset? _editTargetDate;

    [ObservableProperty]
    private int? _editEarliestMonthOffset;

    [ObservableProperty]
    private int? _editLatestMonthOffset;

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
            EditName = value.Name;
            EditAmount = value.Amount;
            EditTargetDate = new DateTimeOffset(value.TargetDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            EditEarliestMonthOffset = value.EarliestMonthOffset;
            EditLatestMonthOffset = value.LatestMonthOffset;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnEditNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnEditAmountChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnEditTargetDateChanged(DateTimeOffset? value) => ScheduleValidateAndSave();
    partial void OnEditEarliestMonthOffsetChanged(int? value) => ScheduleValidateAndSave();
    partial void OnEditLatestMonthOffsetChanged(int? value) => ScheduleValidateAndSave();

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
        SetValidationErrors(validationResult.Errors, DtoToVmPropertyMap);
    }

    private CashflowEventDto BuildEventDtoFromForm()
    {
        var targetDate = EditTargetDate.HasValue ? DateOnly.FromDateTime(EditTargetDate.Value.Date) : default;
        return new CashflowEventDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = EditName?.Trim() ?? string.Empty,
            Type = _cashflowType,
            Amount = EditAmount,
            TargetDate = targetDate,
            EarliestMonthOffset = EditEarliestMonthOffset,
            LatestMonthOffset = EditLatestMonthOffset
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
        EditName = string.Empty;
        EditAmount = 0;
        EditTargetDate = new DateTimeOffset(DateTime.SpecifyKind(DateTime.Today.AddYears(1), DateTimeKind.Utc), TimeSpan.Zero);
        EditEarliestMonthOffset = null;
        EditLatestMonthOffset = null;
        ClearValidationErrors();
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        EditTargetDate = new DateTimeOffset(DateTime.SpecifyKind(DateTime.Today.AddYears(1), DateTimeKind.Utc), TimeSpan.Zero);
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildEventDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors, DtoToVmPropertyMap);
        if (!validationResult.IsValid)
            return;

        var targetDate = DateOnly.FromDateTime(EditTargetDate!.Value.Date);
        var list = current.Events.ToList();
        if (EditingId is null)
        {
            var newItem = new CashflowEventDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = EditName.Trim(),
                Type = _cashflowType,
                Amount = EditAmount,
                TargetDate = targetDate,
                EarliestMonthOffset = EditEarliestMonthOffset,
                LatestMonthOffset = EditLatestMonthOffset
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
                Name = EditName.Trim(),
                Amount = EditAmount,
                TargetDate = targetDate,
                EarliestMonthOffset = EditEarliestMonthOffset,
                LatestMonthOffset = EditLatestMonthOffset
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
