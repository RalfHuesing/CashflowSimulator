using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Zeigt den Stammdaten-Dialog (MetaDto bearbeiten) modal und gibt das Ergebnis zurück.
/// Owner muss vor der ersten Nutzung gesetzt werden (z. B. MainWindow in OnLoaded).
/// </summary>
public interface IMetaEditDialogService
{
    void SetOwner(Avalonia.Controls.Window? owner);

    /// <summary>
    /// Dialog anzeigen; gibt den bearbeiteten MetaDto zurück oder null bei Abbrechen.
    /// </summary>
    Task<MetaDto?> ShowEditAsync(MetaDto current, CancellationToken cancellationToken = default);
}
