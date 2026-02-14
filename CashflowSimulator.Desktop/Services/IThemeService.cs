namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Stellt verfügbare Themes bereit und wendet sie zur Laufzeit an.
/// Pro Szenario gespeicherte Theme-ID kommt aus <see cref="Contracts.Dtos.UiSettingsDto.SelectedThemeId"/>.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Wird ausgelöst, nachdem das Theme visuell angewendet wurde (Ende von ApplyThemeCore, UI-Thread).
    /// Ermöglicht z. B. das Neuerzeugen der Einstellungs-View, um ComboBox-Zustand nach Theme-Swap zu vermeiden.
    /// </summary>
    event EventHandler? ThemeApplied;

    /// <summary>
    /// Alle wählbaren Themes (für ComboBox etc.).
    /// </summary>
    IReadOnlyList<ThemeOption> GetAvailableThemes();

    /// <summary>
    /// Standard-Theme-ID, wenn keine oder unbekannte ID gesetzt ist.
    /// </summary>
    string GetDefaultThemeId();

    /// <summary>
    /// Wendet das Theme mit der angegebenen ID an.
    /// Leer oder unbekannte ID → Default-Theme.
    /// </summary>
    void ApplyTheme(string? themeId);
}

/// <summary>
/// Eine wählbare Theme-Option (Anzeigename + interne ID).
/// </summary>
public sealed record ThemeOption(string Id, string DisplayName);
