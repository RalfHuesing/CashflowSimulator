using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.SimulationResult;

/// <summary>
/// Anzeige-ViewModel für das Ergebnis einer Simulation. Lädt Monatsdaten per <see cref="IResultAnalysisService"/> (RunId) asynchron.
/// </summary>
public partial class SimulationResultViewModel : ObservableObject
{
    /// <summary>Run-Id der angezeigten Simulation.</summary>
    public long RunId { get; }

    /// <summary>Vollständiges Ergebnis (RunId + leere Liste; Daten in <see cref="MonthlyResults"/>).</summary>
    public SimulationResultDto Result { get; }

    /// <summary>Monatliche Ergebnisse für DataGrid-Binding (nach <see cref="LoadAsync"/> befüllt).</summary>
    public ObservableCollection<MonthlyResultDto> MonthlyResults { get; }

    /// <summary>True, wenn die Simulation gerade läuft.</summary>
    [ObservableProperty]
    private bool _isRunning;

    /// <summary>Fehlermeldung bei Abbruch (optional).</summary>
    [ObservableProperty]
    private string? _errorMessage;

    private readonly IResultAnalysisService? _resultAnalysisService;

    /// <summary>
    /// Erstellt das ViewModel für den angegebenen Run. Monatsdaten müssen per <see cref="LoadAsync"/> geladen werden.
    /// </summary>
    public SimulationResultViewModel(long runId, IResultAnalysisService? resultAnalysisService)
    {
        RunId = runId;
        _resultAnalysisService = resultAnalysisService;
        MonthlyResults = new ObservableCollection<MonthlyResultDto>();
        Result = new SimulationResultDto { RunId = runId, MonthlyResults = [] };
    }

    /// <summary>
    /// Lädt die monatlichen Ergebnisse aus dem Service in <see cref="MonthlyResults"/>.
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_resultAnalysisService is null) return;
        var list = await _resultAnalysisService.GetMonthlyResultsAsync(RunId, cancellationToken).ConfigureAwait(true);
        MonthlyResults.Clear();
        foreach (var m in list)
            MonthlyResults.Add(m);
    }
}
