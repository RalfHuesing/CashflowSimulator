using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Zentraler State-Service für das aktuell geladene Simulationsprojekt.
/// Einzige Quelle für Projekt und Dateipfad; alle Consumer (Shell, Feature-ViewModels) lesen/aktualisieren darüber.
/// </summary>
public interface ICurrentProjectService
{
    /// <summary>
    /// Aktuelles Projekt (null wenn noch nicht initialisiert).
    /// </summary>
    SimulationProjectDto? Current { get; }

    /// <summary>
    /// Dateipfad der aktuell geladenen/gespeicherten Datei (null wenn ungespeichert).
    /// </summary>
    string? CurrentFilePath { get; }

    /// <summary>
    /// Setzt das aktuelle Projekt und optional den Dateipfad (z. B. nach Load oder Save).
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void SetCurrent(SimulationProjectDto project, string? filePath = null);

    /// <summary>
    /// Aktualisiert nur die Metadaten des aktuellen Projekts (z. B. aus dem Meta-Edit-Bereich).
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateMeta(MetaDto meta);

    /// <summary>
    /// Aktualisiert nur die UI-Einstellungen des aktuellen Projekts (z. B. aus Einstellungen).
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateUiSettings(UiSettingsDto uiSettings);

    /// <summary>
    /// Wird ausgelöst, wenn sich das Projekt oder der Dateipfad geändert hat.
    /// </summary>
    event EventHandler? ProjectChanged;
}
