using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.SimulationResult;

/// <summary>
/// Anzeige-ViewModel für das Ergebnis einer Simulation. Lädt Monatsdaten per <see cref="IResultAnalysisService"/> (RunId).
/// </summary>
public partial class SimulationResultViewModel : ObservableObject
{
    /// <summary>Run-Id der angezeigten Simulation.</summary>
    public long RunId { get; }

    /// <summary>Vollständiges Ergebnis (RunId + leere Liste; Daten in <see cref="MonthlyResults"/>).</summary>
    public SimulationResultDto Result { get; }

    /// <summary>Monatliche Ergebnisse für DataGrid-Binding (aus Service geladen).</summary>
    public ObservableCollection<MonthlyResultDto> MonthlyResults { get; }

    /// <summary>True, wenn die Simulation gerade läuft (Slice 1: synchron, daher kurz).</summary>
    [ObservableProperty]
    private bool _isRunning;

    /// <summary>Fehlermeldung bei Abbruch (optional).</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Erstellt das ViewModel für den angegebenen Run und lädt die Monatsergebnisse aus dem Service.
    /// </summary>
    public SimulationResultViewModel(long runId, IResultAnalysisService resultAnalysisService)
    {
        RunId = runId;
        var list = resultAnalysisService?.GetMonthlyResults(runId) ?? [];
        MonthlyResults = new ObservableCollection<MonthlyResultDto>(list);
        Result = new SimulationResultDto { RunId = runId, MonthlyResults = [] };
    }
}
