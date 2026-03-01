namespace CashflowSimulator.Infrastructure.Storage;

/// <summary>
/// Räumt den Drafts-Basisordner auf: behält nur die N neuesten Unterordner (Sortierung nach Ordnernamen absteigend).
/// Ordnernamen im Format yyyyMMdd-HHmmss_RunId sind lexikalisch sortierbar.
/// </summary>
public static class DraftsFolderCleanup
{
    /// <summary>
    /// Löscht alle Unterordner von <paramref name="draftsBasePath"/> bis auf die <paramref name="keepNewestCount"/> neuesten (nach Namen absteigend).
    /// </summary>
    /// <param name="draftsBasePath">Absoluter Pfad zum Drafts-Basisordner (z. B. AppData\Local\CashflowSimulator\Drafts).</param>
    /// <param name="keepNewestCount">Anzahl der neuesten Ordner, die behalten werden (Standard: 5).</param>
    public static void KeepNewest(string draftsBasePath, int keepNewestCount = 5)
    {
        if (keepNewestCount < 0)
            keepNewestCount = 0;
        if (!Directory.Exists(draftsBasePath))
            return;

        var subdirs = Directory.EnumerateDirectories(draftsBasePath)
            .OrderByDescending(Path.GetFileName)
            .ToList();

        foreach (var dir in subdirs.Skip(keepNewestCount))
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // Ignorieren (z. B. von anderem Prozess geöffnet)
            }
        }
    }
}
