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
    /// Run-Id der zuletzt durchgeführten Simulation (null wenn noch keine oder nach Reset).
    /// Wird nach erfolgreichem <see cref="StartRunAsync"/> bzw. Simulation-Start gesetzt.
    /// </summary>
    long? LastRunId { get; }

    /// <summary>
    /// Pfad zum Run-Ordner der zuletzt durchgeführten Simulation (null wenn noch keine oder In-Memory-Persistenz).
    /// Wird für Diagnose-Snapshots (Diagnostics/) benötigt.
    /// </summary>
    string? LastRunFolderPath { get; }

    /// <summary>
    /// Setzt die Run-Id und optional den Ergebnisordner-Pfad (z. B. nach Simulation-Start).
    /// </summary>
    /// <param name="runId">Run-Id der Simulation.</param>
    /// <param name="resultFolderPath">Optional: Pfad zum Run-Ordner (z. B. Drafts/yyyyMMdd-HHmmss_RunId).</param>
    void SetLastRunId(long runId, string? resultFolderPath = null);

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
    /// Aktualisiert nur die Simulationsparameter des aktuellen Projekts (z. B. aus Eckdaten).
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateParameters(SimulationParametersDto parameters);

    /// <summary>
    /// Aktualisiert die laufenden Cashflows (Streams) des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateStreams(IReadOnlyList<CashflowStreamDto> streams);

    /// <summary>
    /// Aktualisiert die geplanten Cashflow-Events des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateEvents(IReadOnlyList<CashflowEventDto> events);

    /// <summary>
    /// Aktualisiert die ökonomischen Faktoren (Marktdaten) des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateEconomicFactors(IReadOnlyList<EconomicFactorDto> economicFactors);

    /// <summary>
    /// Aktualisiert die Korrelationen zwischen Faktoren des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateCorrelations(IReadOnlyList<CorrelationEntryDto> correlations);

    /// <summary>
    /// Aktualisiert das Portfolio (alle Vermögenswerte) des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdatePortfolio(PortfolioDto portfolio);

    /// <summary>
    /// Aktualisiert die Anlageklassen des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateAssetClasses(IReadOnlyList<AssetClassDto> assetClasses);

    /// <summary>
    /// Aktualisiert die Steuer-Profile des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateTaxProfiles(IReadOnlyList<TaxProfileDto> taxProfiles);

    /// <summary>
    /// Aktualisiert die Strategie-Profile des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateStrategyProfiles(IReadOnlyList<StrategyProfileDto> strategyProfiles);

    /// <summary>
    /// Aktualisiert die Allokationsprofile des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateAllocationProfiles(IReadOnlyList<AllocationProfileDto> allocationProfiles);

    /// <summary>
    /// Aktualisiert die Lebensphasen des aktuellen Projekts.
    /// Löst <see cref="ProjectChanged"/> aus.
    /// </summary>
    void UpdateLifecyclePhases(IReadOnlyList<LifecyclePhaseDto> lifecyclePhases);

    /// <summary>
    /// Wird ausgelöst, wenn sich das Projekt oder der Dateipfad geändert hat.
    /// </summary>
    event EventHandler? ProjectChanged;
}
