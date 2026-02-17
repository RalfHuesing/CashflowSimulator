using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.AllocationProfiles;

/// <summary>
/// Option für ComboBox (Anlageklasse: Id + Name).
/// </summary>
public record AssetClassOption(string Id, string Name);

/// <summary>
/// Zeile für die Bearbeitung eines Allokationsprofil-Eintrags (mutable für DataGrid-Binding).
/// </summary>
public class AllocationProfileEntryRow : ObservableObject
{
    private string _assetClassId = string.Empty;
    private decimal _targetWeight;
    private Action? _onWeightChanged;
    private IList<AssetClassOption>? _assetClassOptions;

    public void SetNotifyCallback(Action? onWeightChanged) => _onWeightChanged = onWeightChanged;

    public void SetAssetClassOptions(IList<AssetClassOption> options) => _assetClassOptions = options;

    public string AssetClassId
    {
        get => _assetClassId;
        set
        {
            if (SetProperty(ref _assetClassId, value))
            {
                _onWeightChanged?.Invoke();
                OnPropertyChanged(nameof(SelectedAssetClassOption));
            }
        }
    }

    public decimal TargetWeight
    {
        get => _targetWeight;
        set
        {
            if (SetProperty(ref _targetWeight, value))
                _onWeightChanged?.Invoke();
        }
    }

    /// <summary>Für ComboBox-Binding: ausgewählte Anlageklasse (Id ↔ Option).</summary>
    public AssetClassOption? SelectedAssetClassOption
    {
        get => _assetClassOptions?.FirstOrDefault(o => o.Id == _assetClassId);
        set
        {
            var id = value?.Id ?? string.Empty;
            if (_assetClassId == id)
                return;
            _assetClassId = id;
            OnPropertyChanged(nameof(AssetClassId));
            _onWeightChanged?.Invoke();
        }
    }
}

/// <summary>
/// ViewModel für Allokationsprofile (Master-Detail). CRUD für benannte Soll-Allokationen mit Gewichtungen pro Anlageklasse.
/// Beim Löschen werden Referenzen in Lebensphasen auf leer gesetzt.
/// </summary>
public partial class AllocationProfilesViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;
    private const double WeightSumTolerance = 0.0001;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private AllocationProfileDto? _selectedItem;

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _editingId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveEntryCommand))]
    [NotifyPropertyChangedFor(nameof(TotalWeightPercentText))]
    [NotifyPropertyChangedFor(nameof(WeightSumWarning))]
    private AllocationProfileEntryRow? _selectedEntry;

    public ObservableCollection<AllocationProfileDto> Items { get; } = [];
    public ObservableCollection<AllocationProfileEntryRow> EntryRows { get; } = [];
    public ObservableCollection<AssetClassOption> AssetClassOptions { get; } = [];

    /// <summary>Summe aller Zielgewichte in Prozent (z. B. "100 %" oder "95 %").</summary>
    public string TotalWeightPercentText => $"{EntryRows.Sum(r => r.TargetWeight) * 100:F1} %";

    /// <summary>Warnung, wenn die Summe nicht 100 % ergibt.</summary>
    public string? WeightSumWarning =>
        (double)Math.Abs(EntryRows.Sum(r => r.TargetWeight) - 1.0m) > WeightSumTolerance
            ? "Die Summe der Zielgewichtungen weicht von 100 % ab. Sie muss exakt 100 % ergeben."
            : null;

    protected override string HelpKeyPrefix => "AllocationProfiles";

    public AllocationProfilesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "AllocationProfiles";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshItems();
        RefreshAssetClassOptions();
    }

    partial void OnSelectedItemChanged(AllocationProfileDto? value)
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
            EntryRows.Clear();
            foreach (var e in value.Entries ?? [])
            {
                var row = new AllocationProfileEntryRow { AssetClassId = e.AssetClassId, TargetWeight = e.TargetWeight };
                row.SetNotifyCallback(NotifyWeightSummaryChanged);
                row.SetAssetClassOptions(AssetClassOptions);
                EntryRows.Add(row);
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSave();

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

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshItems();
        RefreshAssetClassOptions();
    }

    private void RefreshItems()
    {
        var current = _currentProjectService.Current;
        Items.Clear();
        if (current?.AllocationProfiles is null)
            return;
        foreach (var item in current.AllocationProfiles)
            Items.Add(item);
    }

    private void RefreshAssetClassOptions()
    {
        AssetClassOptions.Clear();
        var current = _currentProjectService.Current;
        if (current?.AssetClasses is null)
            return;
        foreach (var c in current.AssetClasses)
            AssetClassOptions.Add(new AssetClassOption(c.Id, c.Name));
    }

    private void ClearForm()
    {
        EditingId = null;
        Id = string.Empty;
        Name = string.Empty;
        EntryRows.Clear();
        SelectedEntry = null;
        ClearValidationErrors();
    }

    private AllocationProfileDto BuildDtoFromForm()
    {
        return new AllocationProfileDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            Entries = EntryRows.Select(r => new AllocationProfileEntryDto { AssetClassId = r.AssetClassId, TargetWeight = r.TargetWeight }).ToList()
        };
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        Id = "Allocation_" + Guid.NewGuid().ToString("N")[..8];
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

        var list = current.AllocationProfiles.ToList();
        if (EditingId is null)
        {
            list.Add(dto);
            _currentProjectService.UpdateAllocationProfiles(list);
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
                UpdateAllocationProfileIdReferencesInPhases(current, oldId, dto.Id);
            _currentProjectService.UpdateAllocationProfiles(list);
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
        var list = current.AllocationProfiles.Where(x => x.Id != deletedId).ToList();
        _currentProjectService.UpdateAllocationProfiles(list);
        ClearAllocationProfileReferencesInPhases(deletedId);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    private void NotifyWeightSummaryChanged()
    {
        OnPropertyChanged(nameof(TotalWeightPercentText));
        OnPropertyChanged(nameof(WeightSumWarning));
        ScheduleValidateAndSave();
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void AddEntry()
    {
        var firstId = AssetClassOptions.FirstOrDefault()?.Id ?? string.Empty;
        var row = new AllocationProfileEntryRow { AssetClassId = firstId, TargetWeight = 0m };
        row.SetNotifyCallback(NotifyWeightSummaryChanged);
        row.SetAssetClassOptions(AssetClassOptions);
        EntryRows.Add(row);
        NotifyWeightSummaryChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEntry))]
    private void RemoveEntry()
    {
        if (SelectedEntry is null)
            return;
        EntryRows.Remove(SelectedEntry);
        SelectedEntry = null;
        NotifyWeightSummaryChanged();
    }

    private void UpdateAllocationProfileIdReferencesInPhases(SimulationProjectDto project, string oldId, string newId)
    {
        if (project.LifecyclePhases is null)
            return;
        var phases = project.LifecyclePhases
            .Select(p => p.AllocationProfileId == oldId ? p with { AllocationProfileId = newId } : p)
            .ToList();
        _currentProjectService.UpdateLifecyclePhases(phases);
    }

    private void ClearAllocationProfileReferencesInPhases(string profileId)
    {
        var current = _currentProjectService.Current;
        if (current?.LifecyclePhases is null)
            return;
        var phases = current.LifecyclePhases
            .Select(p => p.AllocationProfileId == profileId ? p with { AllocationProfileId = string.Empty } : p)
            .ToList();
        _currentProjectService.UpdateLifecyclePhases(phases);
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
    private bool CanRemoveEntry() => SelectedEntry is not null && _currentProjectService.Current is not null;
}
