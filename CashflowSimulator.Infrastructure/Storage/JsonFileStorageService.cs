using System.Text.Json;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Infrastructure.Storage;

/// <summary>
/// Lädt und speichert Objekte als JSON-Dateien. Pfad inkl. Dateiendung wird vom Aufrufer übergeben.
/// </summary>
public class JsonFileStorageService<T> : IStorageService<T>
{
    private readonly JsonSerializerOptions _options;

    public JsonFileStorageService(JsonSerializerOptions? options = null)
    {
        _options = options ?? CreateDefaultOptions();
    }

    public async Task<Result<T>> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Result<T>.Fail("Dateipfad ist leer.");

        try
        {
            await using var stream = File.OpenRead(path);
            var value = await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false);
            if (value is null)
                return Result<T>.Fail("Datei enthält keine gültigen Daten.");
            return Result<T>.Ok(value);
        }
        catch (FileNotFoundException)
        {
            return Result<T>.Fail($"Datei nicht gefunden: {path}");
        }
        catch (JsonException ex)
        {
            return Result<T>.Fail($"Ungültiges JSON: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<T>.Fail($"Fehler beim Lesen: {ex.Message}");
        }
    }

    public async Task<Result> SaveAsync(string path, T data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Result.Fail("Dateipfad ist leer.");
        if (data is null)
            return Result.Fail("Keine Daten zum Speichern.");

        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, data, _options, cancellationToken).ConfigureAwait(false);
            return Result.Ok();
        }
        catch (IOException ex)
        {
            return Result.Fail($"Fehler beim Speichern: {ex.Message}");
        }
    }

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }
}
