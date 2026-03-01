namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Gesamtergebnis einer Simulation: monatliche Ergebnisse von Start bis Ende.
/// Bei SQLite-Persistenz: <see cref="RunId"/> gesetzt, <see cref="MonthlyResults"/> leer (Daten aus Repository/Service).
/// </summary>
public record SimulationResultDto
{
    /// <summary>Run-Id bei DB-Persistenz; sonst null.</summary>
    public long? RunId { get; init; }

    /// <summary>Pfad zum Ergebnisordner (z. B. Drafts/{yyyyMMdd-HHmmss_Run}) mit simulation.db und input_scenario.json; bei In-Memory-Persistenz null.</summary>
    public string? ResultFolderPath { get; init; }

    /// <summary>Monatliche Ergebnisse in Reihenfolge (Monat 0 bis N-1). Bei DB-Persistenz leer.</summary>
    public List<MonthlyResultDto> MonthlyResults { get; init; } = [];
}
