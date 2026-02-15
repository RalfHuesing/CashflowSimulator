using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.StrategyProfiles;

/// <summary>
/// ViewModel für Strategie-Profile (Master-Detail). CRUD für Liquiditätsreserve, Rebalancing, Lookahead.
/// Beim Löschen werden Referenzen in Lebensphasen auf null gesetzt.
/// </summary>
public partial class StrategyProfilesViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private StrategyProfileDto? _selectedItem;

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _cashReserveMonths;

    [ObservableProperty]
    private decimal _rebalancingThreshold;

    [ObservableProperty]
    private int _lookaheadMonths;

    [ObservableProperty]
    private string? _editingId;

    public ObservableCollection<StrategyProfileDto> Items { get; } = [];

    protected override string HelpKeyPrefix => "StrategyProfiles";

    public StrategyProfilesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "StrategyProfiles";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshItems();
    }

    partial void OnSelectedItemChanged(StrategyProfileDto? value)
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
            Id = value.Id;
            Name = value.Name;
            CashReserveMonths = value.CashReserveMonths;
            RebalancingThreshold = value.RebalancingThreshold;
            LookaheadMonths = value.LookaheadMonths;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnIdChanged(string value) => ScheduleValidateAndSave();
    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnCashReserveMonthsChanged(int value) => ScheduleValidateAndSave();
    partial void OnRebalancingThresholdChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnLookaheadMonthsChanged(int value) => ScheduleValidateAndSave();

    private void ScheduleValidateAndSave()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateForm);
    }

    private void ValidateForm()
    {
        var dto = BuildDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
    }

    private void OnProjectChanged(object? sender, EventArgs e) => RefreshItems();

    private void RefreshItems()
    {
        var current = _currentProjectService.Current;
        Items.Clear();
        if (current?.StrategyProfiles is null)
            return;
        foreach (var item in current.StrategyProfiles)
            Items.Add(item);
    }

    private void ClearForm()
    {
        EditingId = null;
        Id = string.Empty;
        Name = string.Empty;
        CashReserveMonths = 3;
        RebalancingThreshold = 0.05m;
        LookaheadMonths = 24;
        ClearValidationErrors();
    }

    private StrategyProfileDto BuildDtoFromForm()
    {
        return new StrategyProfileDto
        {
            Id = Id?.Trim() ?? string.Empty,
            Name = Name?.Trim() ?? string.Empty,
            CashReserveMonths = CashReserveMonths,
            RebalancingThreshold = RebalancingThreshold,
            LookaheadMonths = LookaheadMonths
        };
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        Id = "Strategy_" + Guid.NewGuid().ToString("N")[..8];
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
        if (!validationResult.IsValid)
            return;

        var list = current.StrategyProfiles.ToList();
        if (EditingId is null)
        {
            list.Add(dto);
            _currentProjectService.UpdateStrategyProfiles(list);
            RefreshItems();
            SelectedItem = Items.FirstOrDefault(x => x.Id == dto.Id);
            ClearForm();
        }
        else
        {
            var idx = list.FindIndex(x => x.Id == EditingId);
            if (idx < 0)
                return;
            var oldId = list[idx].Id;
            list[idx] = dto;
            if (oldId != dto.Id)
                UpdateStrategyProfileIdReferencesInPhases(current, oldId, dto.Id);
            _currentProjectService.UpdateStrategyProfiles(list);
            RefreshItems();
            SelectedItem = Items.FirstOrDefault(x => x.Id == dto.Id);
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

        var deletedId = SelectedItem.Id;
        var list = current.StrategyProfiles.Where(x => x.Id != deletedId).ToList();
        _currentProjectService.UpdateStrategyProfiles(list);
        ClearStrategyProfileReferencesInPhases(deletedId);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    private void UpdateStrategyProfileIdReferencesInPhases(SimulationProjectDto project, string oldId, string newId)
    {
        if (project.LifecyclePhases is null)
            return;
        var phases = project.LifecyclePhases
            .Select(p => p.StrategyProfileId == oldId ? p with { StrategyProfileId = newId } : p)
            .ToList();
        _currentProjectService.UpdateLifecyclePhases(phases);
    }

    /// <summary>
    /// Setzt StrategyProfileId in allen Lebensphasen, die auf profileId verweisen, auf leer.
    /// </summary>
    private void ClearStrategyProfileReferencesInPhases(string profileId)
    {
        var current = _currentProjectService.Current;
        if (current?.LifecyclePhases is null)
            return;
        var phases = current.LifecyclePhases
            .Select(p => p.StrategyProfileId == profileId ? p with { StrategyProfileId = string.Empty } : p)
            .ToList();
        _currentProjectService.UpdateLifecyclePhases(phases);
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}
