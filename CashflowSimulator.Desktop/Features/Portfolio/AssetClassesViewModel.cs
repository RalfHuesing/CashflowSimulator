using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Portfolio;

/// <summary>
/// ViewModel für Anlageklassen (Strategy): Master-Liste mit Name und Zielgewichtung,
/// Detail-Formular. Zeigt eine Info/Warnung, wenn die Summe der Gewichte von 100 % abweicht.
/// </summary>
public partial class AssetClassesViewModel : CrudViewModelBase<AssetClassDto>
{
    private const double WeightSumTolerance = 0.0001;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private double _targetWeight = 0.5;

    [ObservableProperty]
    private string? _color;

    /// <summary>Summe aller Zielgewichte in Prozent (z. B. "100 %" oder "95 %").</summary>
    [ObservableProperty]
    private string _totalWeightPercentText = "0 %";

    /// <summary>Warnung/Info, wenn die Summe nicht 100 % ergibt.</summary>
    [ObservableProperty]
    private string? _weightSumWarning;

    protected override string HelpKeyPrefix => "AssetClasses";

    public AssetClassesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(currentProjectService, helpProvider)
    {
        PageHelpKey = "AssetClasses";
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndUpdateWeightSummary();
    partial void OnTargetWeightChanged(double value) => ScheduleValidateAndUpdateWeightSummary();

    private void ScheduleValidateAndUpdateWeightSummary()
    {
        if (IsLoading)
            return;
        ScheduleDebounced(200, UpdateWeightSummary);
    }

    protected override IEnumerable<AssetClassDto> LoadItems()
    {
        var current = CurrentProjectService.Current;
        return current?.AssetClasses ?? [];
    }

    protected override void UpdateProject(IEnumerable<AssetClassDto> items)
    {
        CurrentProjectService.UpdateAssetClasses(items.ToList());
    }

    protected override AssetClassDto BuildDtoFromForm()
    {
        return new AssetClassDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            TargetWeight = Math.Clamp(TargetWeight, 0.0, 1.0),
            Color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim()
        };
    }

    protected override void MapDtoToForm(AssetClassDto dto)
    {
        Name = dto.Name;
        TargetWeight = dto.TargetWeight;
        Color = dto.Color;
    }

    protected override void ClearFormCore()
    {
        Name = string.Empty;
        TargetWeight = 0.5;
        Color = null;
    }

    protected override ValidationResult ValidateDto(AssetClassDto dto)
    {
        // Momentan gibt es keinen AssetClassDtoValidator; bei Bedarf erweitern
        return ValidationResult.Success();
    }

    protected override void OnProjectChanged(object? sender, EventArgs e)
    {
        base.OnProjectChanged(sender, e);
        UpdateWeightSummary();
    }

    protected override void OnItemDeleted(string deletedId)
    {
        ClearAssetClassReferencesInAssets(deletedId);
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

    private void ClearAssetClassReferencesInAssets(string assetClassId)
    {
        var current = CurrentProjectService.Current;
        if (current?.Portfolio is null)
            return;
        var updated = current.Portfolio.Assets.Select(a => a.AssetClassId == assetClassId ? a with { AssetClassId = string.Empty } : a).ToList();
        CurrentProjectService.UpdatePortfolio(current.Portfolio with { Assets = updated });
    }
}
