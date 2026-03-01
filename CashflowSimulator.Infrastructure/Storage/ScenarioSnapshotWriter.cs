using System.Text.Json;
using System.Text.Json.Serialization;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Infrastructure.Storage;

/// <summary>
/// Schreibt einen JSON-Snapshot des Eingabe-Szenarios (SimulationProjectDto) in den Ergebnisordner als input_scenario.json.
/// </summary>
public sealed class ScenarioSnapshotWriter : IScenarioSnapshotWriter
{
    private const string FileName = "input_scenario.json";
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <inheritdoc />
    public async Task WriteAsync(string resultFolderPath, SimulationProjectDto project, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resultFolderPath);
        ArgumentNullException.ThrowIfNull(project);

        var path = Path.Combine(resultFolderPath, FileName);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, project, Options, cancellationToken).ConfigureAwait(false);
    }
}
