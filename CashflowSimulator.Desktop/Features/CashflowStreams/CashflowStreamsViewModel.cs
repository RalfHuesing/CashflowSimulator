using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.CashflowStreams;

/// <summary>
/// ViewModel für laufende Cashflows (Streams), gefiltert nach Einnahmen oder Ausgaben.
/// </summary>
public partial class CashflowStreamsViewModel : ValidatingViewModelBase
{
    private readonly ICurrentProjectService _currentProjectService;
    private readonly CashflowType _cashflowType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private CashflowStreamDto? _selectedItem;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private decimal _editAmount;

    [ObservableProperty]
    private string _editInterval = "Monthly";

    [ObservableProperty]
    private DateTimeOffset? _editStartDate;

    [ObservableProperty]
    private DateTimeOffset? _editEndDate;

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
        EditingId = value.Id;
        EditName = value.Name;
        EditAmount = value.Amount;
        EditInterval = value.Interval;
        EditStartDate = new DateTimeOffset(value.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        EditEndDate = value.EndDate.HasValue ? new DateTimeOffset(value.EndDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : null;
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
        EditName = string.Empty;
        EditAmount = 0;
        EditInterval = "Monthly";
        var start = _currentProjectService.Current?.Parameters.SimulationStart ?? DateOnly.FromDateTime(DateTime.Today);
        EditStartDate = new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        EditEndDate = null;
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        var start = _currentProjectService.Current?.Parameters.SimulationStart ?? DateOnly.FromDateTime(DateTime.Today);
        EditStartDate = new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;
        if (string.IsNullOrWhiteSpace(EditName) || !EditStartDate.HasValue)
            return;
        var startDate = DateOnly.FromDateTime(EditStartDate.Value.Date);
        var endDate = EditEndDate.HasValue ? (DateOnly?)DateOnly.FromDateTime(EditEndDate.Value.Date) : null;

        var list = current.Streams.ToList();
        if (EditingId is null)
        {
            var newItem = new CashflowStreamDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = EditName.Trim(),
                Type = _cashflowType,
                Amount = EditAmount,
                Interval = EditInterval,
                StartDate = startDate,
                EndDate = endDate
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
                Name = EditName.Trim(),
                Amount = EditAmount,
                Interval = EditInterval,
                StartDate = startDate,
                EndDate = endDate
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
