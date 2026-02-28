using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.SimulationResult;

/// <summary>
/// Anzeige-ViewModel für das Ergebnis einer Simulation. Wird nach dem Lauf mit <see cref="SimulationResultDto"/> erstellt und angezeigt.
/// </summary>
public partial class SimulationResultViewModel : ObservableObject
{
    /// <summary>Vollständiges Ergebnis (für spätere Erweiterungen).</summary>
    public SimulationResultDto Result { get; }

    /// <summary>Monatliche Ergebnisse für DataGrid-Binding (read-only).</summary>
    public ObservableCollection<MonthlyResultDto> MonthlyResults { get; }

    /// <summary>True, wenn die Simulation gerade läuft (Slice 1: synchron, daher kurz).</summary>
    [ObservableProperty]
    private bool _isRunning;

    /// <summary>Fehlermeldung bei Abbruch (optional).</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Erstellt das ViewModel mit dem Ergebnis einer abgeschlossenen Simulation.
    /// </summary>
    public SimulationResultViewModel(SimulationResultDto result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        MonthlyResults = new ObservableCollection<MonthlyResultDto>(result.MonthlyResults);
    }
}
