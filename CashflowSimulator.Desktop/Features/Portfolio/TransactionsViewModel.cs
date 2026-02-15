using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common.Extensions;
using CashflowSimulator.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Portfolio;

/// <summary>
/// Eintrag für die flache Transaktionsliste: Transaktion plus Asset-Info.
/// Identifikation über <see cref="TransactionDto.Id"/> (ID-basiert, unabhängig von Sortierung).
/// </summary>
public record TransactionEntry(string AssetId, string AssetName, TransactionDto Transaction);

/// <summary>
/// ViewModel für das Transaktions-Journal: flache Liste aller Transaktionen aus allen Assets,
/// Detail-Formular mit Asset-Auswahl. Sortierung: neueste zuerst.
/// </summary>
public partial class TransactionsViewModel : ValidatingViewModelBase
{
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private TransactionEntry? _selectedItem;

    [ObservableProperty]
    private DateOnly? _date;

    [ObservableProperty]
    private TransactionType _transactionType = TransactionType.Buy;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private decimal _pricePerUnit;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private string? _editingAssetId;

    /// <summary>Id der bearbeiteten Transaktion (null = neue Transaktion).</summary>
    [ObservableProperty]
    private string? _editingTransactionId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditingAssetId))]
    private AssetOption? _selectedAsset;

    partial void OnSelectedAssetChanged(AssetOption? value)
    {
        _editingAssetId = value?.Id;
    }

    /// <summary>Alle Transaktionen aller Assets, neueste zuerst.</summary>
    public ObservableCollection<TransactionEntry> AllTransactions { get; } = [];

    public ObservableCollection<AssetOption> AssetOptions { get; } = [];

    public static IReadOnlyList<EnumDisplayEntry> TransactionTypeOptions { get; } =
        EnumExtensions.ToDisplayList<TransactionType>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TransactionType))]
    private EnumDisplayEntry? _selectedTransactionTypeOption;

    partial void OnSelectedTransactionTypeOptionChanged(EnumDisplayEntry? value)
    {
        if (value?.Value is TransactionType tt)
            _transactionType = tt;
    }

    protected override string HelpKeyPrefix => "Transactions";

    public TransactionsViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "Transactions";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshAssetOptions();
        RefreshTransactions();
    }

    partial void OnSelectedItemChanged(TransactionEntry? value)
    {
        if (value is null)
        {
            ClearForm();
            return;
        }
        EditingAssetId = value.AssetId;
        EditingTransactionId = value.Transaction.Id;
        SelectedAsset = AssetOptions.FirstOrDefault(o => o.Id == value.AssetId);
        var t = value.Transaction;
        Date = t.Date;
        TransactionType = t.Type;
        Quantity = t.Quantity;
        PricePerUnit = t.PricePerUnit;
        TotalAmount = t.TotalAmount;
        TaxAmount = t.TaxAmount;
        SelectedTransactionTypeOption = TransactionTypeOptions.FirstOrDefault(o => Equals(o.Value, t.Type));
    }

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshAssetOptions();
        RefreshTransactions();
    }

    private void RefreshAssetOptions()
    {
        AssetOptions.Clear();
        var assets = _currentProjectService.Current?.Portfolio?.Assets ?? [];
        foreach (var a in assets)
            AssetOptions.Add(new AssetOption(a.Id, a.Name));
    }

    private void RefreshTransactions()
    {
        AllTransactions.Clear();
        var assets = _currentProjectService.Current?.Portfolio?.Assets ?? [];
        var list = new List<TransactionEntry>();
        foreach (var asset in assets)
        {
            foreach (var transaction in asset.Transactions)
                list.Add(new TransactionEntry(asset.Id, asset.Name, transaction));
        }
        foreach (var entry in list.OrderByDescending(e => e.Transaction.Date))
            AllTransactions.Add(entry);
    }

    private void ClearForm()
    {
        EditingAssetId = null;
        EditingTransactionId = null;
        SelectedAsset = null;
        Date = null;
        TransactionType = TransactionType.Buy;
        SelectedTransactionTypeOption = TransactionTypeOptions.FirstOrDefault(o => Equals(o.Value, TransactionType.Buy));
        Quantity = 0;
        PricePerUnit = 0;
        TotalAmount = 0;
        TaxAmount = 0;
        ClearValidationErrors();
    }

    private TransactionDto BuildTransactionFromForm()
    {
        return new TransactionDto
        {
            Id = EditingTransactionId ?? Guid.NewGuid().ToString(),
            Date = Date ?? DateOnly.FromDateTime(DateTime.Today),
            Type = TransactionType,
            Quantity = Quantity,
            PricePerUnit = PricePerUnit,
            TotalAmount = TotalAmount,
            TaxAmount = TaxAmount
        };
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        Date = DateOnly.FromDateTime(DateTime.Today);
        SelectedAsset = AssetOptions.FirstOrDefault();
        EditingAssetId = SelectedAsset?.Id;
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current?.Portfolio is null)
            return;

        var dto = BuildTransactionFromForm();
        var assetId = EditingAssetId ?? SelectedAsset?.Id;
        if (string.IsNullOrEmpty(assetId))
            return;

        var assets = current.Portfolio.Assets.ToList();
        var assetIndex = assets.FindIndex(a => a.Id == assetId);
        if (assetIndex < 0)
            return;

        var editingId = EditingTransactionId;

        if (editingId is null)
        {
            // Neue Transaktion zum gewählten Asset hinzufügen
            var asset = assets[assetIndex];
            var newTransactions = asset.Transactions.ToList();
            newTransactions.Add(dto);
            assets[assetIndex] = asset with { Transactions = newTransactions };
        }
        else
        {
            // Bestehende Transaktion: per Id finden (unabhängig von Sortierung)
            var (oldAssetIndex, transactionIndexInAsset) = FindTransactionIndex(assets, editingId);
            if (oldAssetIndex >= 0 && transactionIndexInAsset >= 0)
            {
                var oldAsset = assets[oldAssetIndex];
                var oldList = oldAsset.Transactions.ToList();
                oldList.RemoveAt(transactionIndexInAsset);
                assets[oldAssetIndex] = oldAsset with { Transactions = oldList };
            }

            var targetAsset = assets[assetIndex];
            var targetList = targetAsset.Transactions.ToList();
            if (oldAssetIndex == assetIndex && transactionIndexInAsset >= 0)
                targetList.Insert(transactionIndexInAsset, dto);
            else
                targetList.Add(dto);
            assets[assetIndex] = targetAsset with { Transactions = targetList };
        }

        _currentProjectService.UpdatePortfolio(current.Portfolio with { Assets = assets });
        RefreshTransactions();
        ClearForm();
    }

    /// <summary>Findet Asset-Index und Listen-Index einer Transaktion anhand ihrer Id.</summary>
    private static (int AssetIndex, int TransactionIndex) FindTransactionIndex(List<AssetDto> assets, string transactionId)
    {
        for (var ai = 0; ai < assets.Count; ai++)
        {
            var idx = assets[ai].Transactions.FindIndex(t => t.Id == transactionId);
            if (idx >= 0)
                return (ai, idx);
        }
        return (-1, -1);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete()
    {
        if (SelectedItem is null)
            return;
        var current = _currentProjectService.Current;
        if (current?.Portfolio is null)
            return;

        var transactionId = SelectedItem.Transaction.Id;
        var assets = current.Portfolio.Assets.ToList();
        var (assetIndex, transactionIndex) = FindTransactionIndex(assets, transactionId);
        if (assetIndex < 0 || transactionIndex < 0)
            return;

        var asset = assets[assetIndex];
        var newTransactions = asset.Transactions.ToList();
        newTransactions.RemoveAt(transactionIndex);
        assets[assetIndex] = asset with { Transactions = newTransactions };
        _currentProjectService.UpdatePortfolio(current.Portfolio with { Assets = assets });

        RefreshTransactions();
        SelectedItem = null;
        ClearForm();
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}

/// <summary>Eintrag für Asset-ComboBox im Transaktionsformular.</summary>
public record AssetOption(string Id, string Display);
