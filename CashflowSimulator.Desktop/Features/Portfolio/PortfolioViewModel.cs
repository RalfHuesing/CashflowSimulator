using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common.Extensions;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Portfolio;

/// <summary>
/// ViewModel für Vermögenswerte (Assets): Master-Liste mit Suche, Detail-Formular.
/// Transaktionen werden hier nicht angezeigt (siehe Transaktionen-View). Enthält aktueller Kurs und Anlageklasse.
/// </summary>
public partial class PortfolioViewModel : CrudViewModelBase<AssetDto>
{
    private const int SearchDebounceMs = 200;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayQuantity))]
    [NotifyPropertyChangedFor(nameof(DisplayTotalValue))]
    private AssetDto? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _isin = string.Empty;

    [ObservableProperty]
    private AssetType _assetType = AssetType.Etf;

    [ObservableProperty]
    private string? _economicFactorId;

    [ObservableProperty]
    private bool _isActiveSavingsInstrument;

    [ObservableProperty]
    private TaxType _taxType = TaxType.EquityFund;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTotalValue))]
    private decimal _currentPrice;

    [ObservableProperty]
    private string? _assetClassId;

    /// <summary>Stückzahl (aus Bestand/Transaktionen, read-only Anzeige).</summary>
    public decimal DisplayQuantity => SelectedItem?.CurrentQuantity ?? 0;

    /// <summary>Aktueller Gesamtwert = aktueller Kurs × Stückzahl.</summary>
    public decimal DisplayTotalValue => CurrentPrice * DisplayQuantity;

    /// <summary>Für ComboBox: ausgewählter Marktfaktor.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EconomicFactorId))]
    private FactorOption? _selectedFactor;

    partial void OnSelectedFactorChanged(FactorOption? value)
    {
        _economicFactorId = value?.Id;
        ScheduleValidateAndSave();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AssetClassId))]
    private AssetClassOption? _selectedAssetClass;

    partial void OnSelectedAssetClassChanged(AssetClassOption? value)
    {
        _assetClassId = value?.Id;
        ScheduleValidateAndSave();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaxType))]
    private EnumDisplayEntry? _selectedTaxTypeOption;

    partial void OnSelectedTaxTypeOptionChanged(EnumDisplayEntry? value)
    {
        if (value?.Value is TaxType tt)
        {
            _taxType = tt;
            ScheduleValidateAndSave();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AssetType))]
    private EnumDisplayEntry? _selectedAssetTypeOption;

    partial void OnSelectedAssetTypeOptionChanged(EnumDisplayEntry? value)
    {
        if (value?.Value is AssetType at)
        {
            _assetType = at;
            ScheduleValidateAndSave();
        }
    }

    /// <summary>Gefilterte Anzeige der Assets (reaktiv auf SearchText).</summary>
    public ObservableCollection<AssetDto> Assets { get; } = [];

    public ObservableCollection<FactorOption> FactorOptions { get; } = [];

    public ObservableCollection<AssetClassOption> AssetClassOptions { get; } = [];

    public static IReadOnlyList<EnumDisplayEntry> TaxTypeOptions { get; } =
        EnumExtensions.ToDisplayList<TaxType>();

    public static IReadOnlyList<EnumDisplayEntry> AssetTypeOptions { get; } =
        EnumExtensions.ToDisplayList<AssetType>();

    protected override string HelpKeyPrefix => "Portfolio";

    public PortfolioViewModel(
        ICurrentProjectService currentProjectService,
        IHelpProvider helpProvider)
        : base(currentProjectService, helpProvider)
    {
        PageHelpKey = "Portfolio";
        RefreshFactorOptions();
        RefreshAssetClassOptions();
        RefreshAndFilterAssets();
    }

    partial void OnSearchTextChanged(string value)
    {
        ScheduleDebounced(SearchDebounceMs, ApplyFilter);
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnIsinChanged(string value) => ScheduleValidateAndSave();
    partial void OnEconomicFactorIdChanged(string? value) => ScheduleValidateAndSave();
    partial void OnIsActiveSavingsInstrumentChanged(bool value) => ScheduleValidateAndSave();
    partial void OnCurrentPriceChanged(decimal value) => ScheduleValidateAndSave();

    protected override IEnumerable<AssetDto> LoadItems()
    {
        var current = CurrentProjectService.Current;
        return current?.Portfolio?.Assets ?? [];
    }

    protected override void UpdateProject(IEnumerable<AssetDto> items)
    {
        var current = CurrentProjectService.Current;
        if (current?.Portfolio is null)
            return;
        CurrentProjectService.UpdatePortfolio(current.Portfolio with { Assets = items.ToList() });
    }

    protected override AssetDto BuildDtoFromForm()
    {
        var current = CurrentProjectService.Current;
        var existing = current?.Portfolio?.Assets?.FirstOrDefault(a => a.Id == EditingId);
        var transactions = existing?.Transactions ?? [];
        var quantity = existing?.CurrentQuantity ?? 0;
        var totalValue = CurrentPrice * quantity;
        return new AssetDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            Isin = Isin?.Trim() ?? string.Empty,
            AssetType = AssetType,
            AssetClassId = AssetClassId ?? string.Empty,
            CurrentPrice = CurrentPrice,
            EconomicFactorId = EconomicFactorId ?? string.Empty,
            IsActiveSavingsInstrument = IsActiveSavingsInstrument,
            TaxType = TaxType,
            CurrentQuantity = quantity,
            CurrentValue = totalValue,
            Transactions = transactions
        };
    }

    protected override void MapDtoToForm(AssetDto dto)
    {
        Name = dto.Name;
        Isin = dto.Isin ?? string.Empty;
        AssetType = dto.AssetType;
        EconomicFactorId = dto.EconomicFactorId;
        IsActiveSavingsInstrument = dto.IsActiveSavingsInstrument;
        TaxType = dto.TaxType;
        CurrentPrice = dto.CurrentPrice;
        AssetClassId = dto.AssetClassId;
        SelectedFactor = FactorOptions.FirstOrDefault(o => o.Id == dto.EconomicFactorId);
        SelectedAssetClass = AssetClassOptions.FirstOrDefault(o => o.Id == dto.AssetClassId);
        SelectedTaxTypeOption = TaxTypeOptions.FirstOrDefault(o => Equals(o.Value, dto.TaxType));
        SelectedAssetTypeOption = AssetTypeOptions.FirstOrDefault(o => Equals(o.Value, dto.AssetType));
    }

    protected override void ClearFormCore()
    {
        Name = string.Empty;
        Isin = string.Empty;
        AssetType = AssetType.Etf;
        SelectedAssetTypeOption = AssetTypeOptions.FirstOrDefault();
        EconomicFactorId = null;
        SelectedFactor = FactorOptions.FirstOrDefault();
        AssetClassId = null;
        SelectedAssetClass = null;
        CurrentPrice = 0;
        IsActiveSavingsInstrument = false;
        TaxType = TaxType.EquityFund;
        SelectedTaxTypeOption = TaxTypeOptions.FirstOrDefault(o => Equals(o.Value, TaxType.EquityFund));
    }

    protected override ValidationResult ValidateDto(AssetDto dto)
    {
        // Momentan gibt es keinen AssetDtoValidator; bei Bedarf erweitern
        return ValidationResult.Success();
    }

    protected override void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshFactorOptions();
        RefreshAssetClassOptions();
        RefreshAndFilterAssets();
    }

    /// <summary>
    /// Überschreibt RefreshItems, um die gefilterte Liste (Assets) statt Items zu aktualisieren.
    /// </summary>
    private void RefreshAndFilterAssets()
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var all = LoadItems();
        var search = (SearchText ?? string.Empty).Trim().ToUpperInvariant();
        Assets.Clear();
        if (string.IsNullOrEmpty(search))
        {
            foreach (var a in all)
                Assets.Add(a);
        }
        else
        {
            foreach (var a in all)
            {
                if ((a.Name ?? string.Empty).ToUpperInvariant().Contains(search) ||
                    (a.Isin ?? string.Empty).ToUpperInvariant().Contains(search))
                    Assets.Add(a);
            }
        }
        // Items synchronisieren (für die Basisklasse)
        Items.Clear();
        foreach (var a in Assets)
            Items.Add(a);
    }

    private void RefreshFactorOptions()
    {
        FactorOptions.Clear();
        var factors = CurrentProjectService.Current?.EconomicFactors;
        if (factors is not null)
        {
            foreach (var f in factors)
                FactorOptions.Add(new FactorOption(f.Id, f.Name));
        }
    }

    private void RefreshAssetClassOptions()
    {
        AssetClassOptions.Clear();
        var classes = CurrentProjectService.Current?.AssetClasses;
        if (classes is not null)
        {
            foreach (var c in classes)
                AssetClassOptions.Add(new AssetClassOption(c.Id, c.Name));
        }
    }
}

/// <summary>Eintrag für Marktfaktor-ComboBox.</summary>
public record FactorOption(string Id, string Display);

/// <summary>Eintrag für Anlageklassen-ComboBox.</summary>
public record AssetClassOption(string Id, string Display);
