using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common.Extensions;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Marktdaten;

/// <summary>
/// ViewModel für die Marktdaten-Seite (stochastische Faktoren).
/// Master-Detail: Liste der Faktoren, Formular für Eigenschaften.
/// Beim Löschen werden Referenzen in Streams/Events auf null gesetzt.
/// </summary>
public partial class MarktdatenViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private EconomicFactorDto? _selectedItem;

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private StochasticModelType _modelType;

    /// <summary>Für ComboBox-Bindung: ausgewählte Modelltyp-Option.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModelType))]
    private EnumDisplayEntry? _selectedModelTypeOption;

    partial void OnSelectedModelTypeOptionChanged(EnumDisplayEntry? value)
    {
        if (value?.Value is StochasticModelType modelType)
        {
            _modelType = modelType;
            ScheduleValidateAndSave();
        }
    }

    [ObservableProperty]
    private decimal _expectedReturn;

    [ObservableProperty]
    private decimal _volatility;

    [ObservableProperty]
    private decimal _meanReversionSpeed;

    [ObservableProperty]
    private decimal _initialValue;

    [ObservableProperty]
    private string? _editingId;

    public ObservableCollection<EconomicFactorDto> Items { get; } = [];

    /// <summary>Optionen für die ComboBox Modelltyp (Wert + Anzeigetext aus Description-Attribut).</summary>
    public static IReadOnlyList<EnumDisplayEntry> ModelTypeOptions { get; } =
        EnumExtensions.ToDisplayList<StochasticModelType>();

    protected override string HelpKeyPrefix => "Marktdaten";

    public MarktdatenViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "Marktdaten";
        _currentProjectService.ProjectChanged += OnProjectChanged;
        RefreshItems();
    }

    partial void OnSelectedItemChanged(EconomicFactorDto? value)
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
            ModelType = value.ModelType;
            SelectedModelTypeOption = ModelTypeOptions.FirstOrDefault(o => Equals(o.Value, value.ModelType));
            ExpectedReturn = (decimal)value.ExpectedReturn;
            Volatility = (decimal)value.Volatility;
            MeanReversionSpeed = (decimal)value.MeanReversionSpeed;
            InitialValue = (decimal)value.InitialValue;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnIdChanged(string value) => ScheduleValidateAndSave();
    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnModelTypeChanged(StochasticModelType value) => ScheduleValidateAndSave();
    partial void OnExpectedReturnChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnVolatilityChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnMeanReversionSpeedChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnInitialValueChanged(decimal value) => ScheduleValidateAndSave();

    private void ScheduleValidateAndSave()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateAndSave);
    }

    private void ValidateAndSave()
    {
        var dto = BuildFactorDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
    }

    private EconomicFactorDto BuildFactorDtoFromForm()
    {
        return new EconomicFactorDto
        {
            Id = Id?.Trim() ?? string.Empty,
            Name = Name?.Trim() ?? string.Empty,
            ModelType = ModelType,
            ExpectedReturn = (double)ExpectedReturn,
            Volatility = (double)Volatility,
            MeanReversionSpeed = (double)MeanReversionSpeed,
            InitialValue = (double)InitialValue
        };
    }

    private void OnProjectChanged(object? sender, EventArgs e) => RefreshItems();

    private void RefreshItems()
    {
        var current = _currentProjectService.Current;
        Items.Clear();
        if (current?.EconomicFactors is null)
            return;
        foreach (var item in current.EconomicFactors)
            Items.Add(item);
    }

    private void ClearForm()
    {
        EditingId = null;
        Id = string.Empty;
        Name = string.Empty;
        ModelType = StochasticModelType.GeometricBrownianMotion;
        SelectedModelTypeOption = ModelTypeOptions.Count > 0 ? ModelTypeOptions[0] : null;
        ExpectedReturn = 0.07m;
        Volatility = 0.15m;
        MeanReversionSpeed = 0m;
        InitialValue = 100m;
        ClearValidationErrors();
    }

    [RelayCommand]
    private void New()
    {
        SelectedItem = null;
        ClearForm();
        Id = "Faktor_" + Guid.NewGuid().ToString("N")[..8];
    }

    [RelayCommand(CanExecute = nameof(HasCurrentProject))]
    private void Save()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var dto = BuildFactorDtoFromForm();
        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
        if (!validationResult.IsValid)
            return;

        var list = current.EconomicFactors.ToList();
        if (EditingId is null)
        {
            list.Add(dto);
            _currentProjectService.UpdateEconomicFactors(list);
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
            // Wenn Id geändert wurde: Referenzen in Streams/Events aktualisieren
            if (oldId != dto.Id)
                UpdateFactorIdReferences(current, oldId, dto.Id);
            _currentProjectService.UpdateEconomicFactors(list);
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
        var list = current.EconomicFactors.Where(x => x.Id != deletedId).ToList();
        _currentProjectService.UpdateEconomicFactors(list);

        // Alle Cashflow-Referenzen auf diesen Faktor auf null setzen
        ClearFactorReferencesInProject(deletedId);

        RefreshItems();
        SelectedItem = null;
        ClearForm();
    }

    /// <summary>
    /// Setzt EconomicFactorId in allen Streams und Events, die auf factorId verweisen, auf null.
    /// </summary>
    private void ClearFactorReferencesInProject(string factorId)
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return;

        var streamsChanged = false;
        var streams = current.Streams.Select(s =>
        {
            if (s.EconomicFactorId == factorId)
            {
                streamsChanged = true;
                return s with { EconomicFactorId = null };
            }
            return s;
        }).ToList();
        if (streamsChanged)
            _currentProjectService.UpdateStreams(streams);

        var eventsChanged = false;
        var events = current.Events.Select(e =>
        {
            if (e.EconomicFactorId == factorId)
            {
                eventsChanged = true;
                return e with { EconomicFactorId = null };
            }
            return e;
        }).ToList();
        if (eventsChanged)
            _currentProjectService.UpdateEvents(events);
    }

    private void UpdateFactorIdReferences(SimulationProjectDto project, string oldId, string newId)
    {
        var streams = project.Streams.Select(s => s.EconomicFactorId == oldId ? s with { EconomicFactorId = newId } : s).ToList();
        _currentProjectService.UpdateStreams(streams);
        var events = project.Events.Select(e => e.EconomicFactorId == oldId ? e with { EconomicFactorId = newId } : e).ToList();
        _currentProjectService.UpdateEvents(events);
    }

    private bool HasCurrentProject() => _currentProjectService.Current is not null;
    private bool CanDelete() => SelectedItem is not null && _currentProjectService.Current is not null;
}
