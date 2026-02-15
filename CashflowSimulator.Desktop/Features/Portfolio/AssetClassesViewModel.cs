using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Portfolio;

/// <summary>
/// ViewModel für Anlageklassen (Strategy): Master-Liste mit Name und Zielgewichtung,
/// Detail-Formular. Zeigt eine Info/Warnung, wenn die Summe der Gewichte von 100 % abweicht.
/// </summary>
public partial class AssetClassesViewModel : ValidatingViewModelBase
{
    private const double WeightSumTolerance = 0.0001;
    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private AssetClassDto? _selectedItem;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private double _targetWeight = 0.5;

    [ObservableProperty]
    private string? _color;

    [ObservableProperty]
    private string? _editingId;

    /// <summary>Summe aller Zielgewichte in Prozent (z. B. "100 %" oder "95 %").</summary>
    [ObservableProperty]
    private string _totalWeightPercentText = "0 %";

    /// <summary>Warnung/Info, wenn die Summe nicht 100 % ergibt.</summary>
    [ObservableProperty]
    private string? _weightSumWarning;

    public ObservableCollection<AssetClassDto> Items { get; } = [];

    protected override string HelpKeyPrefix => "AssetClasses";

    public AssetClassesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "AssetClasses";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshItems();
    }

    partial void OnSelectedItemChanged(AssetClassDto? value)
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
            Name = value.Name;
            TargetWeight = value.TargetWeight;
            Color = value.Color;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndUpdateWeightSummary();
    partial void OnTargetWeightChanged(double value) => ScheduleValidateAndUpdateWeightSummary();

    private void ScheduleValidateAndUpdateWeightSummary()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(200, UpdateWeightSummary);
    }

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshItems();
        UpdateWeightSummary();
    }

    private void RefreshItems()
    {
        var current = _currentProjectService.Current;
        Items.Clear();
        if (current?.AssetClasses is null)
            return;
        foreach (var item in current.AssetClasses)
            Items.Add(item);
        UpdateWeightSummary();
    }

    private void UpdateWeightSummary()
    {
        var sum = Items.Sum(x => x.TargetWeight);
        TotalWeightPercentText = $"{sum * 100:F1} %";
        if (Math.Abs(sum - 1.0) > WeightSumTolerance)
            WeightSumWarning = "Die Summe der Zielgewichtungen weicht von 100 % ab. Ideal ist eine Gesamtgewichtung von 100 %.";
        else
            WeightSumWarning = null;
    }

    private void ClearForm()
    {
        EditingId = null;
        Name = string.Empty;
        TargetWeight = 0.5;
        Color = null;
        ClearValidationErrors();
    }

    private AssetClassDto BuildDtoFromForm()
    {
        return new AssetClassDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            TargetWeight = Math.Clamp(TargetWeight, 0.0, 1.0),
            Color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim()
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

        var dto = BuildDtoFromForm();
        var list = current.AssetClasses.ToList();
        if (EditingId is null)
        {
            list.Add(dto);
            _currentProjectService.UpdateAssetClasses(list);
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
                UpdateAssetClassIdReferences(current, oldId, dto.Id);
            _currentProjectService.UpdateAssetClasses(list);
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
        var list = current.AssetClasses.Where(x => x.Id != deletedId).ToList();
        _currentProjectService.UpdateAssetClasses(list);
        ClearAssetClassReferencesInAssets(deletedId);
        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    private void UpdateAssetClassIdReferences(SimulationProjectDto project, string oldId, string newId)
    {
        var assets = project.Portfolio?.Assets ?? [];
        var updated = assets.Select(a => a.AssetClassId == oldId ? a with { AssetClassId = newId } : a).ToList();
        if (updated.Count > 0)
            _currentProjectService.UpdatePortfolio(project.Portfolio! with { Assets = updated });
    }

    private void ClearAssetClassReferencesInAssets(string assetClassId)
    {
        var current = _currentProjectService.Current;
        if (current?.Portfolio is null)
            return;
        var updated = current.Portfolio.Assets.Select(a => a.AssetClassId == assetClassId ? a with { AssetClassId = string.Empty } : a).ToList();
        _currentProjectService.UpdatePortfolio(current.Portfolio with { Assets = updated });
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}
