namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Plattformunabhängiger Datei-Öffnen-/Speichern-Dialog (implementiert z. B. über Avalonia StorageProvider).
/// Der Aufrufer erhält den gewählten Dateipfad oder null bei Abbrechen.
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Öffnen-Dialog: Nutzer wählt eine Datei. Rückgabe: voller Pfad oder null bei Abbrechen.
    /// </summary>
    Task<string?> OpenAsync(FileDialogOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Speichern-Dialog: Nutzer wählt Speicherort und Dateiname. Rückgabe: voller Pfad oder null bei Abbrechen.
    /// </summary>
    Task<string?> SaveAsync(SaveFileDialogOptions options, CancellationToken cancellationToken = default);
}

/// <summary>
/// Optionen für den Öffnen-Dialog.
/// </summary>
public record FileDialogOptions(string Title, string FileTypeDescription, string Extension);

/// <summary>
/// Optionen für den Speichern-Dialog.
/// </summary>
public record SaveFileDialogOptions(string Title, string FileTypeDescription, string Extension, string? SuggestedFileName = null);
