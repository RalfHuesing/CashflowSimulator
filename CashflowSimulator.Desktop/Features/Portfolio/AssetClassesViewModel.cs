using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Portfolio;

/// <summary>
/// ViewModel für Anlageklassen (Strategy): Master-Liste mit Name, Detail-Formular.
/// Zielgewichtungen werden in den Allokationsprofilen pro Lebensphase definiert.
/// </summary>
public partial class AssetClassesViewModel : CrudViewModelBase<AssetClassDto>
{
    [ObservableProperty]
    private string _name = string.Empty;

    protected override string HelpKeyPrefix => "AssetClasses";

    public AssetClassesViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(currentProjectService, helpProvider)
    {
        PageHelpKey = "AssetClasses";
        RefreshItems();
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
            Name = Name?.Trim() ?? string.Empty
        };
    }

    protected override void MapDtoToForm(AssetClassDto dto)
    {
        Name = dto.Name;
    }

    protected override void ClearFormCore()
    {
        Name = string.Empty;
    }

    protected override ValidationResult ValidateDto(AssetClassDto dto)
    {
        return ValidationResult.Success();
    }

    protected override void OnItemDeleted(string deletedId)
    {
        ClearAssetClassReferencesInAssets(deletedId);
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
