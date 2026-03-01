namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Service zum automatischen Export des View-Zustands von <see cref="IDiagnosticExport"/>-ViewModels
/// als JSON-Snapshot in das aktuelle Run-Verzeichnis (Unterordner Diagnostics/).
/// </summary>
public interface IDiagnosticExportService
{
    /// <summary>
    /// Schreibt den Zustand des angegebenen ViewModels als JSON in Diagnostics/ (Hintergrund-Task).
    /// Wird nur ausgeführt, wenn eine Simulation aktiv ist (LastRunFolderPath gesetzt).
    /// </summary>
    Task ExportAsync(IDiagnosticExport source, CancellationToken cancellationToken = default);
}
