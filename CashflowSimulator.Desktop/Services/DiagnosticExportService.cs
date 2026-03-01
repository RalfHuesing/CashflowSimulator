using System.Text.Json;
using System.Text.Json.Serialization;
using CashflowSimulator.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Schreibt den Zustand von <see cref="IDiagnosticExport"/>-ViewModels als JSON-Snapshot in das Run-Verzeichnis (Diagnostics/).
/// Läuft im Hintergrund (Task.Run), blockiert die UI nicht; serieller Zugriff verhindert IOException bei schnellem View-Wechsel.
/// </summary>
public sealed class DiagnosticExportService : IDiagnosticExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ICurrentProjectService _currentProjectService;
    private readonly ILogger<DiagnosticExportService> _logger;
    private readonly SemaphoreSlim _exportLock = new(1, 1);

    public DiagnosticExportService(
        ICurrentProjectService currentProjectService,
        ILogger<DiagnosticExportService> logger)
    {
        _currentProjectService = currentProjectService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ExportAsync(IDiagnosticExport source, CancellationToken cancellationToken = default)
    {
        var folderPath = _currentProjectService.LastRunFolderPath;
        if (string.IsNullOrWhiteSpace(folderPath))
            return;

        await _exportLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await Task.Run(() => ExportCore(source, folderPath, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _exportLock.Release();
        }
    }

    private void ExportCore(IDiagnosticExport source, string folderPath, CancellationToken cancellationToken)
    {
        try
        {
            var data = source.GetExportData();
            var fileName = source.ExportFileName?.Trim();
            if (string.IsNullOrEmpty(fileName))
                fileName = "diagnostic.json";
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            var diagnosticsDir = Path.Combine(folderPath, "Diagnostics");
            Directory.CreateDirectory(diagnosticsDir);
            var fullPath = Path.Combine(diagnosticsDir, fileName);

            using var stream = File.Create(fullPath);
            JsonSerializer.Serialize(stream, data, JsonOptions);
            _logger.LogDebug("Diagnose-Snapshot geschrieben: {Path}", fullPath);
        }
        catch (OperationCanceledException)
        {
            // Abbruch durch Token – nicht als Fehler loggen
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Schreiben des Diagnose-Snapshots für {FileName}", source.ExportFileName);
        }
    }
}
