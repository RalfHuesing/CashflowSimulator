namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// UI-Einstellungen pro Szenario (z. B. Theme, später weitere Optionen).
/// Wird mit dem Projekt gespeichert und kann an SettingsViewModel gebunden werden.
/// </summary>
public record UiSettingsDto
{
    /// <summary>
    /// ID des ausgewählten Themes (z. B. "Fluent", "FluentWithCustom", "Simple").
    /// Leer = Standard des Theme-Services.
    /// </summary>
    public string SelectedThemeId { get; init; } = string.Empty;
}
