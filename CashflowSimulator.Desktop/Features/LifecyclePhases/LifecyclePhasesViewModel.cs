using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.LifecyclePhases;

/// <summary>
/// Option für ComboBox (Anzeige = Name, Wert = Id).
/// </summary>
public record ProfileOption(string Id, string Name);

/// <summary>
/// Zeile für die Asset-Allokation-Override-Tabelle (Anlageklasse + Zielgewicht).
/// </summary>
public partial class LifecycleOverrideRowViewModel : ObservableObject
{
    [ObservableProperty]
    private ProfileOption? _selectedAssetClass;

    [ObservableProperty]
    private decimal _targetWeight;
}

/// <summary>
/// ViewModel für Lebensphasen (Master-Detail). Startalter, Steuer- und Strategie-Profil, optionale Asset-Overrides.
/// </summary>
public partial class LifecyclePhasesViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private LifecyclePhaseDto? _selectedItem;

    [ObservableProperty]
    private int _startAge;

    [ObservableProperty]
    private string _taxProfileId = string.Empty;

    [ObservableProperty]
    private string _strategyProfileId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaxProfileId))]
    private ProfileOption? _selectedTaxProfile;

    partial void OnSelectedTaxProfileChanged(ProfileOption? value)
    {
        _taxProfileId = value?.Id ?? string.Empty;
        ScheduleValidateForm();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrategyProfileId))]
    private ProfileOption? _selectedStrategyProfile;

    partial void OnSelectedStrategyProfileChanged(ProfileOption? value)
    {
        _strategyProfileId = value?.Id ?? string.Empty;
        ScheduleValidateForm();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveOverrideCommand))]
    private LifecycleOverrideRowViewModel? _selectedOverrideRow;

    public ObservableCollection<LifecyclePhaseDto> Items { get; } = [];
    public ObservableCollection<ProfileOption> TaxProfileOptions { get; } = [];
    public ObservableCollection<ProfileOption> StrategyProfileOptions { get; } = [];
    public ObservableCollection<ProfileOption> AssetClassOptions { get; } = [];
    public ObservableCollection<LifecycleOverrideRowViewModel> OverrideRows { get; } = [];

    protected override string HelpKeyPrefix => "LifecyclePhases";

    public LifecyclePhasesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "LifecyclePhases";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshOptions();
        RefreshItems();
    }

    partial void OnSelectedItemChanged(LifecyclePhaseDto? value)
    {
        if (value is null)
        {
            ClearForm();
            return;
        }
        _isLoading = true;
        try
        {
            StartAge = value.StartAge;
            TaxProfileId = value.TaxProfileId;
            StrategyProfileId = value.StrategyProfileId;
            SelectedTaxProfile = TaxProfileOptions.FirstOrDefault(o => o.Id == value.TaxProfileId);
            SelectedStrategyProfile = StrategyProfileOptions.FirstOrDefault(o => o.Id == value.StrategyProfileId);
            OverrideRows.Clear();
            foreach (var ov in value.AssetAllocationOverrides ?? [])
            {
                OverrideRows.Add(new LifecycleOverrideRowViewModel
                {
                    SelectedAssetClass = AssetClassOptions.FirstOrDefault(o => o.Id == ov.AssetClassId),
                    TargetWeight = ov.TargetWeight
                });
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnStartAgeChanged(int value) => ScheduleValidateForm();

    private void ScheduleValidateForm()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateForm);
    }

    private void ValidateForm()
    {
        var dto = BuildPhaseDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
    }

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshOptions();
        RefreshItems();
        if (SelectedItem is not null)
        {
            var id = SelectedItem.StartAge;
            SelectedTaxProfile = TaxProfileOptions.FirstOrDefault(o => o.Id == TaxProfileId);
            SelectedStrategyProfile = StrategyProfileOptions.FirstOrDefault(o => o.Id == StrategyProfileId);
            var current = _currentProjectService.Current;
            var phase = current?.LifecyclePhases?.FirstOrDefault(p => p.StartAge == id);
            if (phase is not null)
            {
                _isLoading = true;
                try
                {
                    OverrideRows.Clear();
                    foreach (var ov in phase.AssetAllocationOverrides ?? [])
                    {
                        OverrideRows.Add(new LifecycleOverrideRowViewModel
                        {
                            SelectedAssetClass = AssetClassOptions.FirstOrDefault(o => o.Id == ov.AssetClassId),
                            TargetWeight = ov.TargetWeight
                        });
                    }
                }
                finally
                {
                    _isLoading = false;
                }
            }
        }
    }

    private void RefreshOptions()
    {
        var current = _currentProjectService.Current;
        TaxProfileOptions.Clear();
        StrategyProfileOptions.Clear();
        AssetClassOptions.Clear();
        if (current is null)
            return;
        foreach (var t in current.TaxProfiles ?? [])
            TaxProfileOptions.Add(new ProfileOption(t.Id, t.Name));
        foreach (var s in current.StrategyProfiles ?? [])
            StrategyProfileOptions.Add(new ProfileOption(s.Id, s.Name));
        foreach (var c in current.AssetClasses ?? [])
            AssetClassOptions.Add(new ProfileOption(c.Id, c.Name));
    }

    private void RefreshItems()
    {
        var current = _currentProjectService.Current;
        Items.Clear();
        if (current?.LifecyclePhases is null)
            return;
        foreach (var item in current.LifecyclePhases)
            Items.Add(item);
    }

    private void ClearForm()
    {
        StartAge = 0;
        TaxProfileId = string.Empty;
        StrategyProfileId = string.Empty;
        SelectedTaxProfile = null;
        SelectedStrategyProfile = null;
        OverrideRows.Clear();
        ClearValidationErrors();
    }

    private LifecyclePhaseDto BuildPhaseDtoFromForm()
    {
        var overrides = OverrideRows
            .Where(r => !string.IsNullOrEmpty(r.SelectedAssetClass?.Id))
            .Select(r => new AssetAllocationOverrideDto
            {
                AssetClassId = r.SelectedAssetClass!.Id,
                TargetWeight = r.TargetWeight
            })
            .ToList();
        return new LifecyclePhaseDto
        {
            StartAge = StartAge,
            TaxProfileId = TaxProfileId?.Trim() ?? string.Empty,
            StrategyProfileId = StrategyProfileId?.Trim() ?? string.Empty,
            AssetAllocationOverrides = overrides
        };
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildPhaseDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
        if (!validationResult.IsValid)
            return;

        var list = current.LifecyclePhases.ToList();
        if (SelectedItem is null)
        {
            list.Add(dto);
            _currentProjectService.UpdateLifecyclePhases(list);
            RefreshItems();
            SelectedItem = Items.FirstOrDefault(x => x.StartAge == dto.StartAge);
            ClearForm();
        }
        else
        {
            var idx = list.FindIndex(p => p.StartAge == SelectedItem.StartAge);
            if (idx < 0)
                return;
            list[idx] = dto;
            _currentProjectService.UpdateLifecyclePhases(list);
            RefreshItems();
            SelectedItem = Items.FirstOrDefault(x => x.StartAge == dto.StartAge);
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

        var toRemove = SelectedItem.StartAge;
        var list = current.LifecyclePhases.Where(p => p.StartAge != toRemove).ToList();
        _currentProjectService.UpdateLifecyclePhases(list);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void AddOverride()
    {
        OverrideRows.Add(new LifecycleOverrideRowViewModel
        {
            SelectedAssetClass = AssetClassOptions.FirstOrDefault(),
            TargetWeight = 0
        });
    }

    [RelayCommand(CanExecute = nameof(CanRemoveOverride))]
    private void RemoveOverride()
    {
        if (SelectedOverrideRow is null)
            return;
        OverrideRows.Remove(SelectedOverrideRow);
        SelectedOverrideRow = null;
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
    private bool CanRemoveOverride() => SelectedOverrideRow is not null;
}
