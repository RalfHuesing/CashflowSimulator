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
/// ViewModel für Lebensphasen (Master-Detail). Startalter, Steuer- und Strategie-Profil.
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
    private string _allocationProfileId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllocationProfileId))]
    private ProfileOption? _selectedAllocationProfile;

    partial void OnSelectedAllocationProfileChanged(ProfileOption? value)
    {
        _allocationProfileId = value?.Id ?? string.Empty;
        ScheduleValidateAndSave();
    }

    [ObservableProperty]
    private int _glidepathMonths;

    partial void OnGlidepathMonthsChanged(int value) => ScheduleValidateAndSave();

    public ObservableCollection<ProfileOption> TaxProfileOptions { get; } = [];
    public ObservableCollection<ProfileOption> StrategyProfileOptions { get; } = [];
    public ObservableCollection<ProfileOption> AllocationProfileOptions { get; } = [];

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
        return new LifecyclePhaseDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            StartAge = StartAge,
            TaxProfileId = TaxProfileId?.Trim() ?? string.Empty,
            StrategyProfileId = StrategyProfileId?.Trim() ?? string.Empty,
            AssetAllocationOverrides = [],
            AllocationProfileId = AllocationProfileId?.Trim() ?? string.Empty,
            GlidepathMonths = GlidepathMonths
        };
    }

    /// <inheritdoc />
    protected override void MapDtoToForm(LifecyclePhaseDto dto)
    {
        StartAge = dto.StartAge;
        TaxProfileId = dto.TaxProfileId;
        StrategyProfileId = dto.StrategyProfileId;
        AllocationProfileId = dto.AllocationProfileId ?? string.Empty;
        GlidepathMonths = dto.GlidepathMonths;
        SelectedTaxProfile = TaxProfileOptions.FirstOrDefault(o => o.Id == dto.TaxProfileId);
        SelectedStrategyProfile = StrategyProfileOptions.FirstOrDefault(o => o.Id == dto.StrategyProfileId);
        SelectedAllocationProfile = AllocationProfileOptions.FirstOrDefault(o => o.Id == dto.AllocationProfileId);
    }

    /// <inheritdoc />
    protected override void ClearFormCore()
    {
        StartAge = 0;
        TaxProfileId = string.Empty;
        StrategyProfileId = string.Empty;
        AllocationProfileId = string.Empty;
        GlidepathMonths = 0;
        SelectedTaxProfile = null;
        SelectedStrategyProfile = null;
        SelectedAllocationProfile = null;
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
        AllocationProfileOptions.Clear();
        if (current is null)
            return;
        foreach (var t in current.TaxProfiles ?? [])
            TaxProfileOptions.Add(new ProfileOption(t.Id, t.Name));
        foreach (var s in current.StrategyProfiles ?? [])
            StrategyProfileOptions.Add(new ProfileOption(s.Id, s.Name));
        AllocationProfileOptions.Add(new ProfileOption("", "—"));
        foreach (var a in current.AllocationProfiles ?? [])
            AllocationProfileOptions.Add(new ProfileOption(a.Id, a.Name));
    }
}
