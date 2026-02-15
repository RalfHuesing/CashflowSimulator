using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.TaxProfiles;

/// <summary>
/// ViewModel für Steuer-Profile (Master-Detail). CRUD für Kapitalertragsteuer, Freibetrag, Einkommensteuer-Satz.
/// Beim Löschen werden Referenzen in Lebensphasen auf null gesetzt.
/// </summary>
public partial class TaxProfilesViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private TaxProfileDto? _selectedItem;

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _capitalGainsTaxRate;

    [ObservableProperty]
    private decimal _taxFreeAllowance;

    [ObservableProperty]
    private decimal _incomeTaxRate;

    [ObservableProperty]
    private string? _editingId;

    public ObservableCollection<TaxProfileDto> Items { get; } = [];

    protected override string HelpKeyPrefix => "TaxProfiles";

    public TaxProfilesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "TaxProfiles";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshItems();
    }

    partial void OnSelectedItemChanged(TaxProfileDto? value)
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
            CapitalGainsTaxRate = value.CapitalGainsTaxRate;
            TaxFreeAllowance = value.TaxFreeAllowance;
            IncomeTaxRate = value.IncomeTaxRate;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnIdChanged(string value) => ScheduleValidateAndSave();
    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnCapitalGainsTaxRateChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnTaxFreeAllowanceChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnIncomeTaxRateChanged(decimal value) => ScheduleValidateAndSave();

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
        if (current?.TaxProfiles is null)
            return;
        foreach (var item in current.TaxProfiles)
            Items.Add(item);
    }

    private void ClearForm()
    {
        EditingId = null;
        Id = string.Empty;
        Name = string.Empty;
        CapitalGainsTaxRate = 0.26375m;
        TaxFreeAllowance = 1000m;
        IncomeTaxRate = 0.35m;
        ClearValidationErrors();
    }

    private TaxProfileDto BuildDtoFromForm()
    {
        return new TaxProfileDto
        {
            Id = Id?.Trim() ?? string.Empty,
            Name = Name?.Trim() ?? string.Empty,
            CapitalGainsTaxRate = CapitalGainsTaxRate,
            TaxFreeAllowance = TaxFreeAllowance,
            IncomeTaxRate = IncomeTaxRate
        };
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        Id = "Tax_" + Guid.NewGuid().ToString("N")[..8];
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

        var list = current.TaxProfiles.ToList();
        if (EditingId is null)
        {
            list.Add(dto);
            _currentProjectService.UpdateTaxProfiles(list);
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
                UpdateTaxProfileIdReferencesInPhases(current, oldId, dto.Id);
            _currentProjectService.UpdateTaxProfiles(list);
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
        var list = current.TaxProfiles.Where(x => x.Id != deletedId).ToList();
        _currentProjectService.UpdateTaxProfiles(list);
        ClearTaxProfileReferencesInPhases(deletedId);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    /// <summary>
    /// Aktualisiert TaxProfileId in allen Lebensphasen von oldId auf newId (bei Umbenennung).
    /// </summary>
    private void UpdateTaxProfileIdReferencesInPhases(SimulationProjectDto project, string oldId, string newId)
    {
        if (project.LifecyclePhases is null)
            return;
        var phases = project.LifecyclePhases
            .Select(p => p.TaxProfileId == oldId ? p with { TaxProfileId = newId } : p)
            .ToList();
        _currentProjectService.UpdateLifecyclePhases(phases);
    }

    /// <summary>
    /// Setzt TaxProfileId in allen Lebensphasen, die auf profileId verweisen, auf leer.
    /// </summary>
    private void ClearTaxProfileReferencesInPhases(string profileId)
    {
        var current = _currentProjectService.Current;
        if (current?.LifecyclePhases is null)
            return;
        var phases = current.LifecyclePhases
            .Select(p => p.TaxProfileId == profileId ? p with { TaxProfileId = string.Empty } : p)
            .ToList();
        _currentProjectService.UpdateLifecyclePhases(phases);
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}
