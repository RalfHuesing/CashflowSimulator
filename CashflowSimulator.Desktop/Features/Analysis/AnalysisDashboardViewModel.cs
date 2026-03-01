using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop.Features.Analysis;

/// <summary>
/// ViewModel für das Analyse-Dashboard: jährliche Aggregation, Plausibilitäts-Journal (Master-Detail) und Gesamtvermögens-Chart.
/// </summary>
public partial class AnalysisDashboardViewModel : ObservableObject, IDiagnosticExport
{
    private readonly long _runId;
    private readonly IResultAnalysisService? _resultAnalysisService;
    private readonly ILogger<AnalysisDashboardViewModel> _logger;
    private List<MonthlyResultDto> _allMonths = [];

    public AnalysisDashboardViewModel(
        long runId,
        IResultAnalysisService? resultAnalysisService,
        ILogger<AnalysisDashboardViewModel> logger)
    {
        _runId = runId;
        _resultAnalysisService = resultAnalysisService;
        _logger = logger;
        YearlySummaries = new ObservableCollection<YearlySummaryItem>();
        MonthsInSelectedYear = new ObservableCollection<MonthlyResultDto>();
        TotalAssetsSeries = new ObservableCollection<ISeries>();
    }

    /// <summary>Run-Id der angezeigten Simulation (0 = kein Run, leere Ansicht).</summary>
    public long RunId => _runId;

    /// <summary>True, wenn keine Daten geladen sind (RunId 0 oder leere Ergebnisliste).</summary>
    [ObservableProperty]
    private bool _isEmpty;

    /// <summary>Jährlich aggregierte Zeilen für die Master-Liste (Plausibilitäts-Journal).</summary>
    public ObservableCollection<YearlySummaryItem> YearlySummaries { get; }

    /// <summary>Ausgewähltes Jahr (Master-Selection) für Detail-Ansicht.</summary>
    [ObservableProperty]
    private YearlySummaryItem? _selectedYearlySummary;

    /// <summary>Monate des ausgewählten Jahres für Detail-DataGrid.</summary>
    public ObservableCollection<MonthlyResultDto> MonthsInSelectedYear { get; }

    /// <summary>Ausgewählter Monat (Detail-Selection) für CashflowSnapshots.</summary>
    [ObservableProperty]
    private MonthlyResultDto? _selectedMonth;

    /// <summary>Reihen für das Linien-Diagramm (Gesamtvermögen über Zeit).</summary>
    public ObservableCollection<ISeries> TotalAssetsSeries { get; }

    /// <summary>
    /// Lädt Monatsdaten aus dem Service, baut Jahresaggregate und Chart-Series auf.
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        YearlySummaries.Clear();
        MonthsInSelectedYear.Clear();
        TotalAssetsSeries.Clear();
        SelectedYearlySummary = null;
        SelectedMonth = null;

        if (_runId == 0 || _resultAnalysisService is null)
        {
            IsEmpty = true;
            return;
        }

        try
        {
            var list = await _resultAnalysisService.GetMonthlyResultsAsync(_runId, cancellationToken).ConfigureAwait(true);
            _allMonths = list.ToList();
            if (_allMonths.Count == 0)
            {
                IsEmpty = true;
                return;
            }

            IsEmpty = false;
            BuildYearlySummaries();
            BuildTotalAssetsSeries();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Analyse-Daten für RunId {RunId}", _runId);
            IsEmpty = true;
        }
    }

    partial void OnSelectedYearlySummaryChanged(YearlySummaryItem? value)
    {
        MonthsInSelectedYear.Clear();
        SelectedMonth = null;
        if (value is null) return;

        var startMonth = value.YearIndex * 12;
        var endMonth = Math.Min(startMonth + 12, _allMonths.Count);
        for (var i = startMonth; i < endMonth; i++)
            MonthsInSelectedYear.Add(_allMonths[i]);
    }

    private void BuildYearlySummaries()
    {
        var yearGroups = _allMonths
            .GroupBy(m => m.MonthIndex / 12)
            .OrderBy(g => g.Key);
        foreach (var grp in yearGroups)
        {
            var last = grp.OrderBy(m => m.MonthIndex).Last();
            YearlySummaries.Add(new YearlySummaryItem
            {
                YearIndex = grp.Key,
                EndCashBalance = last.CashBalance,
                EndTotalAssets = last.TotalAssets
            });
        }
    }

    private void BuildTotalAssetsSeries()
    {
        var values = _allMonths
            .OrderBy(m => m.MonthIndex)
            .Select(m => (double)m.TotalAssets)
            .ToList();
        TotalAssetsSeries.Add(new LineSeries<double>
        {
            Name = "Gesamtvermögen",
            Values = values,
            Fill = null
        });
    }

    /// <inheritdoc />
    public object GetExportData() => new AnalysisDashboardDiagnosticDto(
        _runId,
        [.. YearlySummaries],
        _allMonths);

    /// <inheritdoc />
    public string ExportFileName => "analysis-dashboard.json";
}

/// <summary>
/// DTO für den Diagnose-Snapshot des Analyse-Dashboards.
/// </summary>
internal sealed record AnalysisDashboardDiagnosticDto(
    long RunId,
    List<YearlySummaryItem> YearlySummaries,
    List<MonthlyResultDto> MonthlyResults);
