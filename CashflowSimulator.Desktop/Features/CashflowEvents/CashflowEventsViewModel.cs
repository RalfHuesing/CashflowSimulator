using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.CashflowEvents;

/// <summary>
/// ViewModel für geplante Cashflow-Events, gefiltert nach Einnahmen oder Ausgaben.
/// Validierung über <see cref="ValidationRunner"/>; Fehler nur im Info-Panel.
/// Property-Namen 1:1 wie im DTO (Rules-konform).
/// </summary>
public partial class CashflowEventsViewModel : CrudViewModelBase<CashflowEventDto>
{
    private readonly CashflowType _cashflowType;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private DateOnly? _targetDate;

    [ObservableProperty]
    private int? _earliestMonthOffset;

    [ObservableProperty]
    private int? _latestMonthOffset;

    /// <summary>Optional: Marktfaktor zur Dynamisierung (null = Keine).</summary>
    [ObservableProperty]
    private string? _economicFactorId;

    /// <summary>Für ComboBox: „Keine" + alle Marktfaktoren.</summary>
    public ObservableCollection<DynamicFactorOption> DynamicFactorOptions { get; } = [];

    [ObservableProperty]
    private DynamicFactorOption? _selectedDynamicFactor;

    partial void OnSelectedDynamicFactorChanged(DynamicFactorOption? value)
    {
        _economicFactorId = value?.Id;
        ScheduleValidateAndSave();
    }

    public string Title => _cashflowType == CashflowType.Income ? "Geplante Einnahmen" : "Geplante Ausgaben";

    /// <inheritdoc />
    protected override string HelpKeyPrefix => "CashflowEvents";

    public CashflowEventsViewModel(
        ICurrentProjectService currentProjectService,
        IHelpProvider helpProvider,
        CashflowType cashflowType)
        : base(currentProjectService, helpProvider)
    {
        _cashflowType = cashflowType;
        PageHelpKey = "CashflowEvents";
        RefreshDynamicFactorOptions();
        RefreshItems();
    }

    partial void OnNameChanged(string value) => ScheduleValidateAndSave();
    partial void OnAmountChanged(decimal value) => ScheduleValidateAndSave();
    partial void OnTargetDateChanged(DateOnly? value) => ScheduleValidateAndSave();
    partial void OnEarliestMonthOffsetChanged(int? value) => ScheduleValidateAndSave();
    partial void OnLatestMonthOffsetChanged(int? value) => ScheduleValidateAndSave();

    protected override IEnumerable<CashflowEventDto> LoadItems()
    {
        var current = CurrentProjectService.Current;
        if (current?.Events is null)
            return [];
        return current.Events.Where(e => e.Type == _cashflowType);
    }

    protected override void UpdateProject(IEnumerable<CashflowEventDto> items)
    {
        var current = CurrentProjectService.Current;
        if (current is null)
            return;

        // Wichtig: Die Liste enthält nur Items des aktuellen Typs (_cashflowType).
        // Wir müssen sie mit den Items des anderen Typs mergen.
        var otherTypeItems = current.Events.Where(e => e.Type != _cashflowType).ToList();
        var mergedList = otherTypeItems.Concat(items).ToList();
        CurrentProjectService.UpdateEvents(mergedList);
    }

    protected override CashflowEventDto BuildDtoFromForm()
    {
        return new CashflowEventDto
        {
            Id = EditingId ?? Guid.NewGuid().ToString(),
            Name = Name?.Trim() ?? string.Empty,
            Type = _cashflowType,
            Amount = Amount,
            TargetDate = TargetDate.GetValueOrDefault(),
            EarliestMonthOffset = EarliestMonthOffset,
            LatestMonthOffset = LatestMonthOffset,
            EconomicFactorId = EconomicFactorId
        };
    }

    protected override void MapDtoToForm(CashflowEventDto dto)
    {
        Name = dto.Name;
        Amount = dto.Amount;
        TargetDate = dto.TargetDate;
        EarliestMonthOffset = dto.EarliestMonthOffset;
        LatestMonthOffset = dto.LatestMonthOffset;
        EconomicFactorId = dto.EconomicFactorId;
        SelectedDynamicFactor = DynamicFactorOptions.FirstOrDefault(o => o.Id == dto.EconomicFactorId);
    }

    protected override void ClearFormCore()
    {
        Name = string.Empty;
        Amount = 0;
        TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
        EarliestMonthOffset = null;
        LatestMonthOffset = null;
        EconomicFactorId = null;
        SelectedDynamicFactor = DynamicFactorOptions.FirstOrDefault(o => o.Id is null);
    }

    protected override ValidationResult ValidateDto(CashflowEventDto dto)
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
        TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
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
