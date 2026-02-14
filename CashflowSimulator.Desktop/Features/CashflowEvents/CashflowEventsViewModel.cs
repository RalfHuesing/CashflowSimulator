using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.CashflowEvents;

/// <summary>
/// ViewModel für geplante Cashflow-Events, gefiltert nach Einnahmen oder Ausgaben.
/// </summary>
public partial class CashflowEventsViewModel : ValidatingViewModelBase
{
    private readonly ICurrentProjectService _currentProjectService;
    private readonly CashflowType _cashflowType;

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
        EditingId = value.Id;
        EditName = value.Name;
        EditAmount = value.Amount;
        EditTargetDate = new DateTimeOffset(value.TargetDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        EditEarliestMonthOffset = value.EarliestMonthOffset;
        EditLatestMonthOffset = value.LatestMonthOffset;
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
        EditTargetDate = new DateTimeOffset(DateTime.Today.AddYears(1), TimeSpan.Zero);
        EditEarliestMonthOffset = null;
        EditLatestMonthOffset = null;
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        EditTargetDate = new DateTimeOffset(DateTime.Today.AddYears(1), TimeSpan.Zero);
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;
        if (string.IsNullOrWhiteSpace(EditName) || !EditTargetDate.HasValue)
            return;
        var targetDate = DateOnly.FromDateTime(EditTargetDate.Value.Date);

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
