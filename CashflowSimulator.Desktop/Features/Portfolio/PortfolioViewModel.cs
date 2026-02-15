using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common.Extensions;
using CashflowSimulator.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Portfolio;

/// <summary>
/// ViewModel für Vermögenswerte (Assets): Master-Liste mit Suche, Detail-Formular.
/// Transaktionen werden hier nicht angezeigt (siehe Transaktionen-View). Enthält aktueller Kurs und Anlageklasse.
/// </summary>
public partial class PortfolioViewModel : ValidatingViewModelBase
{
    private const int SearchDebounceMs = 200;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
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
        ScheduleValidateAndSaveIfNeeded();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AssetClassId))]
    private AssetClassOption? _selectedAssetClass;

    partial void OnSelectedAssetClassChanged(AssetClassOption? value)
    {
        _assetClassId = value?.Id;
        ScheduleValidateAndSaveIfNeeded();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaxType))]
    private EnumDisplayEntry? _selectedTaxTypeOption;

    partial void OnSelectedTaxTypeOptionChanged(EnumDisplayEntry? value)
    {
        if (value?.Value is TaxType tt)
        {
            _taxType = tt;
            ScheduleValidateAndSaveIfNeeded();
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
            ScheduleValidateAndSaveIfNeeded();
        }
    }

    /// <summary>Id des Assets im Formular (null = Neu).</summary>
    [ObservableProperty]
    private string? _editingId;

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
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "Portfolio";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshFactorOptions();
        RefreshAssetClassOptions();
        RefreshAndFilterAssets();
    }

    partial void OnSearchTextChanged(string value)
    {
        ScheduleDebounced(SearchDebounceMs, ApplyFilter);
    }

    partial void OnSelectedItemChanged(AssetDto? value)
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
            Isin = value.Isin ?? string.Empty;
            AssetType = value.AssetType;
            EconomicFactorId = value.EconomicFactorId;
            IsActiveSavingsInstrument = value.IsActiveSavingsInstrument;
            TaxType = value.TaxType;
            CurrentPrice = value.CurrentPrice;
            AssetClassId = value.AssetClassId;
            SelectedFactor = FactorOptions.FirstOrDefault(o => o.Id == value.EconomicFactorId);
            SelectedAssetClass = AssetClassOptions.FirstOrDefault(o => o.Id == value.AssetClassId);
            SelectedTaxTypeOption = TaxTypeOptions.FirstOrDefault(o => Equals(o.Value, value.TaxType));
            SelectedAssetTypeOption = AssetTypeOptions.FirstOrDefault(o => Equals(o.Value, value.AssetType));
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSaveIfNeeded();
    partial void OnIsinChanged(string value) => ScheduleValidateAndSaveIfNeeded();
    partial void OnEconomicFactorIdChanged(string? value) => ScheduleValidateAndSaveIfNeeded();
    partial void OnIsActiveSavingsInstrumentChanged(bool value) => ScheduleValidateAndSaveIfNeeded();
    partial void OnCurrentPriceChanged(decimal value) => ScheduleValidateAndSaveIfNeeded();

    private void ScheduleValidateAndSaveIfNeeded()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(300, () => { /* Validierung vorbereitet für spätere Asset-Validatoren */ });
    }

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshFactorOptions();
        RefreshAssetClassOptions();
        RefreshAndFilterAssets();
    }

    private void RefreshFactorOptions()
    {
        FactorOptions.Clear();
        var factors = _currentProjectService.Current?.EconomicFactors;
        if (factors is not null)
        {
            foreach (var f in factors)
                FactorOptions.Add(new FactorOption(f.Id, f.Name));
        }
    }

    private void RefreshAssetClassOptions()
    {
        AssetClassOptions.Clear();
        var classes = _currentProjectService.Current?.AssetClasses;
        if (classes is not null)
        {
            foreach (var c in classes)
                AssetClassOptions.Add(new AssetClassOption(c.Id, c.Name));
        }
    }

    private void RefreshAndFilterAssets()
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var current = _currentProjectService.Current;
        var all = current?.Portfolio?.Assets ?? [];
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
    }

    private void ClearForm()
    {
        EditingId = null;
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
        ClearValidationErrors();
    }

    private AssetDto BuildAssetFromForm()
    {
        var current = _currentProjectService.Current;
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
        if (current?.Portfolio is null)
            return;

        var dto = BuildAssetFromForm();
        var list = current.Portfolio.Assets.ToList();
        if (EditingId is null)
        {
            list.Add(dto);
            _currentProjectService.UpdatePortfolio(current.Portfolio with { Assets = list });
            RefreshAndFilterAssets();
            SelectedItem = Assets.FirstOrDefault(x => x.Id == dto.Id);
            ClearForm();
        }
        else
        {
            var idx = list.FindIndex(x => x.Id == EditingId);
            if (idx < 0)
                return;
            list[idx] = dto;
            _currentProjectService.UpdatePortfolio(current.Portfolio with { Assets = list });
            RefreshAndFilterAssets();
            SelectedItem = Assets.FirstOrDefault(x => x.Id == EditingId);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete()
    {
        if (SelectedItem is null)
            return;
        var current = _currentProjectService.Current;
        if (current?.Portfolio is null)
            return;
        var list = current.Portfolio.Assets.Where(x => x.Id != SelectedItem.Id).ToList();
        _currentProjectService.UpdatePortfolio(current.Portfolio with { Assets = list });
        RefreshAndFilterAssets();
        SelectedItem = null;
        ClearForm();
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}

/// <summary>Eintrag für Marktfaktor-ComboBox.</summary>
public record FactorOption(string Id, string Display);

/// <summary>Eintrag für Anlageklassen-ComboBox.</summary>
public record AssetClassOption(string Id, string Display);
