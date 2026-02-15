using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
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
/// Nutzt <see cref="CrudViewModelBase{TDto}"/> für CRUD; ID-basierte Identifikation.
/// </summary>
public partial class LifecyclePhasesViewModel : CrudViewModelBase<LifecyclePhaseDto>
{
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
        ScheduleValidateAndSave();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrategyProfileId))]
    private ProfileOption? _selectedStrategyProfile;

    partial void OnSelectedStrategyProfileChanged(ProfileOption? value)
    {
        _strategyProfileId = value?.Id ?? string.Empty;
        ScheduleValidateAndSave();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveOverrideCommand))]
    private LifecycleOverrideRowViewModel? _selectedOverrideRow;

    public ObservableCollection<ProfileOption> TaxProfileOptions { get; } = [];
    public ObservableCollection<ProfileOption> StrategyProfileOptions { get; } = [];
    public ObservableCollection<ProfileOption> AssetClassOptions { get; } = [];
    public ObservableCollection<LifecycleOverrideRowViewModel> OverrideRows { get; } = [];

    protected override string HelpKeyPrefix => "LifecyclePhases";

    public LifecyclePhasesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(currentProjectService, helpProvider)
    {
        PageHelpKey = "LifecyclePhases";
        RefreshOptions();
        RefreshItems();
    }

    partial void OnStartAgeChanged(int value) => ScheduleValidateAndSave();

    /// <inheritdoc />
    protected override IEnumerable<LifecyclePhaseDto> LoadItems()
    {
        var current = CurrentProjectService.Current;
        if (current?.LifecyclePhases is null)
            return [];
        return current.LifecyclePhases;
    }

    /// <inheritdoc />
    protected override void UpdateProject(IEnumerable<LifecyclePhaseDto> items)
    {
        CurrentProjectService.UpdateLifecyclePhases(items.ToList());
    }

    /// <inheritdoc />
    protected override LifecyclePhaseDto BuildDtoFromForm()
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
            Id = EditingId ?? Guid.NewGuid().ToString(),
            StartAge = StartAge,
            TaxProfileId = TaxProfileId?.Trim() ?? string.Empty,
            StrategyProfileId = StrategyProfileId?.Trim() ?? string.Empty,
            AssetAllocationOverrides = overrides
        };
    }

    /// <inheritdoc />
    protected override void MapDtoToForm(LifecyclePhaseDto dto)
    {
        StartAge = dto.StartAge;
        TaxProfileId = dto.TaxProfileId;
        StrategyProfileId = dto.StrategyProfileId;
        SelectedTaxProfile = TaxProfileOptions.FirstOrDefault(o => o.Id == dto.TaxProfileId);
        SelectedStrategyProfile = StrategyProfileOptions.FirstOrDefault(o => o.Id == dto.StrategyProfileId);
        OverrideRows.Clear();
        foreach (var ov in dto.AssetAllocationOverrides ?? [])
        {
            OverrideRows.Add(new LifecycleOverrideRowViewModel
            {
                SelectedAssetClass = AssetClassOptions.FirstOrDefault(o => o.Id == ov.AssetClassId),
                TargetWeight = ov.TargetWeight
            });
        }
    }

    /// <inheritdoc />
    protected override void ClearFormCore()
    {
        StartAge = 0;
        TaxProfileId = string.Empty;
        StrategyProfileId = string.Empty;
        SelectedTaxProfile = null;
        SelectedStrategyProfile = null;
        OverrideRows.Clear();
        ClearValidationErrors();
    }

    /// <inheritdoc />
    protected override ValidationResult ValidateDto(LifecyclePhaseDto dto)
    {
        return ValidationRunner.Validate(dto);
    }

    /// <inheritdoc />
    protected override void OnProjectChanged(object? sender, EventArgs e)
    {
        var editingId = SelectedItem?.Id;
        RefreshOptions();
        base.OnProjectChanged(sender, e);
        if (editingId is not null)
            SelectedItem = Items.FirstOrDefault(x => x.Id == editingId);
    }

    private void RefreshOptions()
    {
        var current = CurrentProjectService.Current;
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

    private bool HasCurrentProject() => CurrentProjectService.Current is not null;
    private bool CanRemoveOverride() => SelectedOverrideRow is not null;
}
