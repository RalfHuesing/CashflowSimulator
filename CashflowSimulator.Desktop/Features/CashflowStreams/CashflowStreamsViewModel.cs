using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common.Extensions;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.CashflowStreams;

/// <summary>
/// ViewModel für laufende Cashflows (Streams), gefiltert nach Einnahmen oder Ausgaben.
/// Validierung über <see cref="ValidationRunner"/>; Fehler nur im Info-Panel.
/// Property-Namen 1:1 wie im DTO (Rules-konform).
/// </summary>
public partial class CashflowStreamsViewModel : CrudViewModelBase<CashflowStreamDto>
{
    private readonly CashflowType _cashflowType;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private CashflowInterval _interval = CashflowInterval.Monthly;

    /// <summary>Für ComboBox-Bindung: ausgewählte Intervall-Option (Description-Anzeige).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private EnumDisplayEntry? _selectedIntervalOption;

    partial void OnSelectedIntervalOptionChanged(EnumDisplayEntry? value)
    {
        if (value?.Value is CashflowInterval interval)
        {
            _interval = interval;
            ScheduleValidateAndSave();
        }
    }

    [ObservableProperty]
    private DateOnly? _startDate;

    [ObservableProperty]
    private DateOnly? _endDate;

    /// <summary>Optional: Marktfaktor zur Dynamisierung (null = Keine).</summary>
    [ObservableProperty]
    private string? _economicFactorId;

    /// <summary>Für ComboBox: „Keine" + alle Marktfaktoren.</summary>
    [ObservableProperty]
    private DynamicFactorOption? _selectedDynamicFactor;

    partial void OnSelectedDynamicFactorChanged(DynamicFactorOption? value)
    {
        _economicFactorId = value?.Id;
        ScheduleValidateAndSave();
    }

    public ObservableCollection<DynamicFactorOption> DynamicFactorOptions { get; } = [];

    public string Title => _cashflowType == CashflowType.Income ? "Laufende Einnahmen" : "Laufende Ausgaben";

    /// <summary>Optionen für die ComboBox Intervall (Wert + Anzeigetext aus Description-Attribut).</summary>
    public static IReadOnlyList<EnumDisplayEntry> IntervalOptions { get; } =
        EnumExtensions.ToDisplayList<CashflowInterval>();

    /// <inheritdoc />
    protected override string HelpKeyPrefix => "CashflowStreams";

    public CashflowStreamsViewModel(
        ICurrentProjectService currentProjectService,
        IHelpProvider helpProvider,
        CashflowType cashflowType)
        : base(currentProjectService, helpProvider)
    {
        _cashflowType = cashflowType;
        PageHelpKey = "CashflowStreams";
        RefreshDynamicFactorOptions();
        RefreshItems();
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnAmountChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnIntervalChanged(CashflowInterval value) => ScheduleValidateAndSave();
    partial void OnStartDateChanged(DateOnly? value) => ScheduleValidateAndSave();
    partial void OnEndDateChanged(DateOnly? value) => ScheduleValidateAndSave();

    protected override IEnumerable<CashflowStreamDto> LoadItems()
    {
        var current = CurrentProjectService.Current;
        if (current?.Streams is null)
            return [];
        return current.Streams.Where(s => s.Type == _cashflowType);
    }

    protected override void UpdateProject(IEnumerable<CashflowStreamDto> items)
    {
        var current = CurrentProjectService.Current;
        if (current is null)
            return;

        // Wichtig: Die Liste enthält nur Items des aktuellen Typs (_cashflowType).
        // Wir müssen sie mit den Items des anderen Typs mergen.
        var otherTypeItems = current.Streams.Where(s => s.Type != _cashflowType).ToList();
        var mergedList = otherTypeItems.Concat(items).ToList();
        CurrentProjectService.UpdateStreams(mergedList);
    }

    protected override CashflowStreamDto BuildDtoFromForm()
    {
        return new CashflowStreamDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            Type = _cashflowType,
            Amount = Amount,
            Interval = Interval,
            StartDate = StartDate.GetValueOrDefault(),
            EndDate = EndDate,
            EconomicFactorId = EconomicFactorId
        };
    }

    protected override void MapDtoToForm(CashflowStreamDto dto)
    {
        Name = dto.Name;
        Amount = dto.Amount;
        Interval = dto.Interval;
        StartDate = dto.StartDate;
        EndDate = dto.EndDate;
        EconomicFactorId = dto.EconomicFactorId;
        SelectedIntervalOption = IntervalOptions.FirstOrDefault(o => Equals(o.Value, dto.Interval));
        SelectedDynamicFactor = DynamicFactorOptions.FirstOrDefault(o => o.Id == dto.EconomicFactorId);
    }

    protected override void ClearFormCore()
    {
        Name = string.Empty;
        Amount = 0;
        Interval = CashflowInterval.Monthly;
        SelectedIntervalOption = IntervalOptions.Count > 0 ? IntervalOptions[0] : null;
        var start = CurrentProjectService.Current?.Parameters.SimulationStart ?? DateOnly.FromDateTime(DateTime.Today);
        StartDate = start;
        EndDate = null;
        EconomicFactorId = null;
        SelectedDynamicFactor = DynamicFactorOptions.FirstOrDefault(o => o.Id is null);
    }

    protected override ValidationResult ValidateDto(CashflowStreamDto dto)
    {
        return ValidationRunner.Validate(dto);
    }

    protected override void OnProjectChanged(object? sender, EventArgs e)
    {
        RefreshDynamicFactorOptions();
        base.OnProjectChanged(sender, e);
    }

    protected override void OnNewItemCreated()
    {
        var start = CurrentProjectService.Current?.Parameters.SimulationStart ?? DateOnly.FromDateTime(DateTime.Today);
        StartDate = start;
    }

    private void RefreshDynamicFactorOptions()
    {
        DynamicFactorOptions.Clear();
        DynamicFactorOptions.Add(new DynamicFactorOption(null, "Keine"));
        var factors = CurrentProjectService.Current?.EconomicFactors;
        if (factors is not null)
        {
            foreach (var f in factors)
                DynamicFactorOptions.Add(new DynamicFactorOption(f.Id, f.Name));
        }
    }
}

/// <summary>Eintrag für Dynamisierung-ComboBox (Id null = „Keine").</summary>
public record DynamicFactorOption(string? Id, string Display);
