using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common.Extensions;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Marktdaten;

/// <summary>
/// ViewModel für die Marktdaten-Seite (stochastische Faktoren).
/// Master-Detail: Liste der Faktoren, Formular für Eigenschaften.
/// Beim Löschen werden Referenzen in Streams/Events auf null gesetzt.
/// </summary>
public partial class MarktdatenViewModel : CrudViewModelBase<EconomicFactorDto>
{
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

    /// <summary>Optionen für die ComboBox Modelltyp (Wert + Anzeigetext aus Description-Attribut).</summary>
    public static IReadOnlyList<EnumDisplayEntry> ModelTypeOptions { get; } =
        EnumExtensions.ToDisplayList<StochasticModelType>();

    protected override string HelpKeyPrefix => "Marktdaten";

    public MarktdatenViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(currentProjectService, helpProvider)
    {
        PageHelpKey = "Marktdaten";
    }

    partial void OnIdChanged(string value) => ScheduleValidateAndSave();
    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnModelTypeChanged(StochasticModelType value) => ScheduleValidateAndSave();
    partial void OnExpectedReturnChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnVolatilityChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnMeanReversionSpeedChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnInitialValueChanged(decimal value) => ScheduleValidateAndSave();

    protected override IEnumerable<EconomicFactorDto> LoadItems()
    {
        var current = CurrentProjectService.Current;
        return current?.EconomicFactors ?? [];
    }

    protected override void UpdateProject(IEnumerable<EconomicFactorDto> items)
    {
        CurrentProjectService.UpdateEconomicFactors(items.ToList());
    }

    protected override EconomicFactorDto BuildDtoFromForm()
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

    protected override void MapDtoToForm(EconomicFactorDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        ModelType = dto.ModelType;
        SelectedModelTypeOption = ModelTypeOptions.FirstOrDefault(o => Equals(o.Value, dto.ModelType));
        ExpectedReturn = (decimal)dto.ExpectedReturn;
        Volatility = (decimal)dto.Volatility;
        MeanReversionSpeed = (decimal)dto.MeanReversionSpeed;
        InitialValue = (decimal)dto.InitialValue;
    }

    protected override void ClearFormCore()
    {
        Id = string.Empty;
        Name = string.Empty;
        ModelType = StochasticModelType.GeometricBrownianMotion;
        SelectedModelTypeOption = ModelTypeOptions.Count > 0 ? ModelTypeOptions[0] : null;
        ExpectedReturn = 0.07m;
        Volatility = 0.15m;
        MeanReversionSpeed = 0m;
        InitialValue = 100m;
    }

    protected override ValidationResult ValidateDto(EconomicFactorDto dto)
    {
        return ValidationRunner.Validate(dto);
    }

    protected override void OnNewItemCreated()
    {
        Id = "Faktor_" + Guid.NewGuid().ToString("N")[..8];
    }

    protected override void OnItemDeleted(string deletedId)
    {
        // Alle Cashflow-Referenzen auf diesen Faktor auf null setzen
        ClearFactorReferencesInProject(deletedId);
    }

    /// <summary>
    /// Setzt EconomicFactorId in allen Streams und Events, die auf factorId verweisen, auf null.
    /// </summary>
    private void ClearFactorReferencesInProject(string factorId)
    {
        var current = CurrentProjectService.Current;
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
            CurrentProjectService.UpdateStreams(streams);

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
            CurrentProjectService.UpdateEvents(events);
    }
}
