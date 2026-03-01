namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Markiert ViewModels, deren Zustand für die Diagnose (z. B. KI-Fehleranalyse) als JSON-Snapshot exportiert werden kann.
/// Wird beim Wechsel in den entsprechenden Analyse-View automatisch ausgelöst, wenn eine Simulation aktiv ist.
/// </summary>
public interface IDiagnosticExport
{
    /// <summary>
    /// Liefert das zu serialisierende DTO (wird als JSON in Diagnostics/ geschrieben).
    /// </summary>
    object GetExportData();

    /// <summary>
    /// Dateiname für den Snapshot (z. B. analysis-dashboard.json).
    /// </summary>
    string ExportFileName { get; }
}
