using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Korrelationen;

/// <summary>
/// ViewModel für die Korrelationen-Seite. Flache Liste der Korrelationseinträge (Faktor A ↔ Faktor B).
/// Detail: Auswahl der beiden Faktoren (ComboBox) und Korrelation (-1 bis 1).
/// Zeigt Matrix-Inkonsistenzen (nicht positiv definit) im Validierungsbereich an.
/// </summary>
public partial class KorrelationenViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private CorrelationEntryDto? _selectedItem;

    [ObservableProperty]
    private FactorOption? _selectedFactorA;

    [ObservableProperty]
    private FactorOption? _selectedFactorB;

    [ObservableProperty]
    private decimal _correlation;

    [ObservableProperty]
    private string? _editingKey;

    public ObservableCollection<CorrelationEntryDto> Items { get; } = [];

    /// <summary>Faktoren für ComboBox (Id + Name).</summary>
    public ObservableCollection<FactorOption> FactorOptions { get; } = [];

    protected override string HelpKeyPrefix => "Korrelationen";

    public KorrelationenViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "Korrelationen";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshFactorOptions();
        RefreshItems();
    }

    partial void OnSelectedItemChanged(CorrelationEntryDto? value)
    {
        if (value is null)
        {
            ClearForm();
            return;
        }
        _isLoading = true;
        try
        {
            EditingKey = GetEntryKey(value);
            SelectedFactorA = FactorOptions.FirstOrDefault(o => o.Id == value.FactorIdA);
            SelectedFactorB = FactorOptions.FirstOrDefault(o => o.Id == value.FactorIdB);
            Correlation = (decimal)value.Correlation;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnSelectedFactorAChanged(FactorOption? value) => ScheduleValidateAndSave();
    partial void OnSelectedFactorBChanged(FactorOption? value) => ScheduleValidateAndSave();
    partial void OnCorrelationChanged(decimal value) => ScheduleValidateAndSave();

    private void ScheduleValidateAndSave()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateAndSave);
    }

    private void ValidateAndSave()
    {
        var dto = BuildEntryDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);

        var matrixError = GetMatrixErrorIfAny(dto);
        if (matrixError is not null)
        {
            var errors = new List<Contracts.Common.ValidationError>(validationResult.Errors ?? [])
            {
                new(ValidatingViewModelBase.FormLevelErrorsKey, matrixError)
            };
            SetValidationErrors(errors);
        }
    }

    private string? GetMatrixErrorIfAny(CorrelationEntryDto? pendingEntry)
    {
        var current = _currentProjectService.Current;
        if (current?.EconomicFactors is null || current.EconomicFactors.Count < 2)
            return null;

        var correlations = current.Correlations?.ToList() ?? [];
        if (pendingEntry is not null && !string.IsNullOrEmpty(pendingEntry.FactorIdA) && !string.IsNullOrEmpty(pendingEntry.FactorIdB))
        {
            var key = GetEntryKey(pendingEntry);
            correlations = correlations.Where(c => GetEntryKey(c) != key).ToList();
            correlations.Add(pendingEntry);
        }

        var project = current with { Correlations = correlations };
        return CorrelationMatrixValidation.GetPositiveDefinitenessError(project);
    }

    private static string GetEntryKey(CorrelationEntryDto e)
    {
        var a = e.FactorIdA ?? string.Empty;
        var b = e.FactorIdB ?? string.Empty;
        return string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
    }

    private CorrelationEntryDto BuildEntryDtoFromForm()
    {
        return new CorrelationEntryDto
        {
            FactorIdA = SelectedFactorA?.Id ?? string.Empty,
            FactorIdB = SelectedFactorB?.Id ?? string.Empty,
            Correlation = (double)Correlation
        };
    }

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshFactorOptions();
        RefreshItems();
    }

    private void RefreshFactorOptions()
    {
        FactorOptions.Clear();
        var factors = _currentProjectService.Current?.EconomicFactors;
        if (factors is null)
            return;
        foreach (var f in factors)
            FactorOptions.Add(new FactorOption(f.Id, f.Name));
    }

    private void RefreshItems()
    {
        Items.Clear();
        var list = _currentProjectService.Current?.Correlations;
        if (list is null)
            return;
        foreach (var item in list)
            Items.Add(item);
    }

    private void ClearForm()
    {
        EditingKey = null;
        SelectedFactorA = null;
        SelectedFactorB = null;
        Correlation = 0;
        ClearValidationErrors();
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        if (FactorOptions.Count >= 2)
        {
            SelectedFactorA = FactorOptions[0];
            SelectedFactorB = FactorOptions[1];
        }
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildEntryDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
        if (!validationResult.IsValid)
            return;

        var matrixError = GetMatrixErrorIfAny(dto);
        if (matrixError is not null)
        {
            SetValidationErrors([new ValidationError(ValidatingViewModelBase.FormLevelErrorsKey, matrixError)]);
            return;
        }

        var list = current.Correlations?.ToList() ?? [];
        var key = GetEntryKey(dto);
        list.RemoveAll(c => GetEntryKey(c) == key);
        list.Add(dto);
        _currentProjectService.UpdateCorrelations(list);
        RefreshItems();
        SelectedItem = Items.FirstOrDefault(c => GetEntryKey(c) == key);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete()
    {
        if (SelectedItem is null)
            return;
        var current = _currentProjectService.Current;
        if (current is null)
            return;
        var key = GetEntryKey(SelectedItem);
        var list = current.Correlations?.Where(c => GetEntryKey(c) != key).ToList() ?? [];
        _currentProjectService.UpdateCorrelations(list);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}

/// <summary>Eintrag für Faktor-ComboBox.</summary>
public record FactorOption(string Id, string Name);
