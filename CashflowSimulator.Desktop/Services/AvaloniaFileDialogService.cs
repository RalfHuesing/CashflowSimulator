using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Implementierung von <see cref="IFileDialogService"/> über Avalonias <see cref="IStorageProvider"/> (plattformunabhängig).
/// Der Besitzer (TopLevel) muss vor der ersten Nutzung per <see cref="SetOwner"/> gesetzt werden (z. B. in App nach Erstellung des MainWindow).
/// </summary>
public class AvaloniaFileDialogService : IFileDialogService
{
    private volatile TopLevel? _owner;

    /// <summary>
    /// Setzt das Fenster, in dessen Kontext die Dialoge geöffnet werden (üblicherweise MainWindow).
    /// Wird vom Host (App) nach Erstellung des MainWindow aufgerufen.
    /// </summary>
    public void SetOwner(TopLevel? owner) => _owner = owner;

    /// <inheritdoc />
    public async Task<string?> OpenAsync(FileDialogOptions options, CancellationToken cancellationToken = default)
    {
        var topLevel = _owner;
        if (topLevel?.StorageProvider?.CanOpen != true)
            return null;

        var pickerOptions = new FilePickerOpenOptions
        {
            Title = options.Title,
            FileTypeFilter = [CreateFileType(options.FileTypeDescription, options.Extension)]
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(pickerOptions).WaitAsync(cancellationToken).ConfigureAwait(true);
        var file = files.Count > 0 ? files[0] : null;
        return file is null ? null : file.TryGetLocalPath();
    }

    /// <inheritdoc />
    public async Task<string?> SaveAsync(SaveFileDialogOptions options, CancellationToken cancellationToken = default)
    {
        var topLevel = _owner;
        if (topLevel?.StorageProvider?.CanSave != true)
            return null;

        var pickerOptions = new FilePickerSaveOptions
        {
            Title = options.Title,
            SuggestedFileName = options.SuggestedFileName ?? "Szenario.json",
            DefaultExtension = options.Extension.TrimStart('.'),
            FileTypeChoices = [CreateFileType(options.FileTypeDescription, options.Extension)]
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(pickerOptions).WaitAsync(cancellationToken).ConfigureAwait(true);
        return file?.TryGetLocalPath();
    }

    private static FilePickerFileType CreateFileType(string description, string extension)
    {
        var ext = extension.StartsWith('.') ? extension : "." + extension;
        return new FilePickerFileType(description) { Patterns = [ "*" + ext ] };
    }
}
