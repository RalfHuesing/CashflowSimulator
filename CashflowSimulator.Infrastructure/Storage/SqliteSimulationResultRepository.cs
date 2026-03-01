using System.Collections.Concurrent;
using System.Globalization;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using Microsoft.Data.Sqlite;

namespace CashflowSimulator.Infrastructure.Storage;

/// <summary>
/// SQLite-Repository für Simulationsergebnisse. Ein Run = ein Ordner im Drafts-Verzeichnis mit simulation.db.
/// Speicherort: AppData/Local/CashflowSimulator/Drafts/{yyyyMMdd-HHmmss_RunId}/. Vor jedem StartRunAsync wird aufgeräumt (nur 5 neueste Ordner).
/// </summary>
public sealed class SqliteSimulationResultRepository : ISimulationResultRepository
{
    private const string DbFileName = "simulation.db";
    private const int KeepNewestDraftCount = 5;

    private static string DefaultDraftsBasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CashflowSimulator",
        "Drafts");

    private readonly string _draftsBasePath;
    private long _nextRunId;
    private readonly ConcurrentDictionary<long, string> _runIdToFolder = new();

    /// <summary>
    /// Erstellt das Repository mit dem Standard-Drafts-Pfad (AppData/Local/CashflowSimulator/Drafts).
    /// </summary>
    public SqliteSimulationResultRepository() : this(DefaultDraftsBasePath) { }

    /// <summary>
    /// Erstellt das Repository mit einem benutzerdefinierten Drafts-Basisordner (z. B. für Tests).
    /// </summary>
    public SqliteSimulationResultRepository(string draftsBasePath)
    {
        _draftsBasePath = draftsBasePath ?? DefaultDraftsBasePath;
    }

    /// <inheritdoc />
    public async Task<RunStartResult> StartRunAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_draftsBasePath))
            Directory.CreateDirectory(_draftsBasePath);
        DraftsFolderCleanup.KeepNewest(_draftsBasePath, KeepNewestDraftCount);

        var runId = Interlocked.Increment(ref _nextRunId);

        await using var memoryConnection = new SqliteConnection("Data Source=:memory:");
        await memoryConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await CreateSchemaAsync(memoryConnection, cancellationToken).ConfigureAwait(false);

        await using (var cmd = memoryConnection.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO Runs (Id, CreatedAtUtc) VALUES (@id, datetime('now'));";
            cmd.Parameters.AddWithValue("@id", runId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        var folderName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_{runId}";
        var runFolderPath = Path.Combine(_draftsBasePath, folderName);
        Directory.CreateDirectory(runFolderPath);

        var dbPath = Path.Combine(runFolderPath, DbFileName);
        await using (var fileConnection = new SqliteConnection($"Data Source={dbPath}"))
        {
            await fileConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await Task.Run(() => memoryConnection.BackupDatabase(fileConnection), cancellationToken).ConfigureAwait(false);
        }

        _runIdToFolder[runId] = runFolderPath;
        return new RunStartResult(runId, runFolderPath);
    }

    /// <inheritdoc />
    public async Task WriteMonthlyResultsAsync(long runId, IEnumerable<MonthlyResultDto> entries, CancellationToken cancellationToken = default)
    {
        var path = GetDbPath(runId);
        if (path is null || !File.Exists(path))
            throw new InvalidOperationException($"Run {runId} nicht gefunden (DB-Datei fehlt).");

        var entriesList = entries.ToList();
        if (entriesList.Count == 0)
            return;

        await using var connection = new SqliteConnection($"Data Source={path}");
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var tx = connection.BeginTransaction();
        try
        {
            foreach (var entry in entriesList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = """
                        INSERT INTO MonthlyResults (RunId, MonthIndex, Age, CashBalance, TotalAssets)
                        VALUES (@runId, @monthIndex, @age, @cashBalance, @totalAssets)
                        """;
                    cmd.Parameters.AddWithValue("@runId", runId);
                    cmd.Parameters.AddWithValue("@monthIndex", entry.MonthIndex);
                    cmd.Parameters.AddWithValue("@age", entry.Age);
                    cmd.Parameters.AddWithValue("@cashBalance", entry.CashBalance.ToString(CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@totalAssets", entry.TotalAssets.ToString(CultureInfo.InvariantCulture));
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                var snapshots = entry.CashflowSnapshots ?? [];
                for (var i = 0; i < snapshots.Count; i++)
                {
                    var s = snapshots[i];
                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = """
                        INSERT INTO Snapshots (RunId, MonthIndex, Sequence, Name, CashflowType, Amount)
                        VALUES (@runId, @monthIndex, @seq, @name, @cashflowType, @amount)
                        """;
                    cmd.Parameters.AddWithValue("@runId", runId);
                    cmd.Parameters.AddWithValue("@monthIndex", entry.MonthIndex);
                    cmd.Parameters.AddWithValue("@seq", i);
                    cmd.Parameters.AddWithValue("@name", s.Name ?? "");
                    cmd.Parameters.AddWithValue("@cashflowType", (int)s.CashflowType);
                    cmd.Parameters.AddWithValue("@amount", s.Amount.ToString(CultureInfo.InvariantCulture));
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public Task CompleteRunAsync(long runId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonthlyResultDto>> GetMonthlyResultsAsync(long runId, CancellationToken cancellationToken = default)
    {
        var path = GetDbPath(runId);
        if (path is null || !File.Exists(path))
            return [];

        var results = new List<MonthlyResultDto>();
        await using var connection = new SqliteConnection($"Data Source={path}");
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = """
                SELECT MonthIndex, Age, CashBalance, TotalAssets
                FROM MonthlyResults
                WHERE RunId = @runId
                ORDER BY MonthIndex
                """;
            cmd.Parameters.AddWithValue("@runId", runId);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var monthIndex = reader.GetInt32(0);
                var age = reader.GetDouble(1);
                var cashStr = reader.GetString(2);
                var assetsStr = reader.GetString(3);
                var cash = decimal.Parse(cashStr, CultureInfo.InvariantCulture);
                var assets = decimal.Parse(assetsStr, CultureInfo.InvariantCulture);
                var snapshots = await GetSnapshotsAsync(connection, runId, monthIndex, cancellationToken).ConfigureAwait(false);
                results.Add(new MonthlyResultDto
                {
                    MonthIndex = monthIndex,
                    Age = age,
                    CashBalance = cash,
                    TotalAssets = assets,
                    CashflowSnapshots = snapshots
                });
            }
        }

        return results;
    }

    private string? GetDbPath(long runId) =>
        _runIdToFolder.TryGetValue(runId, out var folder) ? Path.Combine(folder, DbFileName) : null;

    private static async Task CreateSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var sql in SchemaStatements)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<List<CashflowSnapshotEntryDto>> GetSnapshotsAsync(SqliteConnection connection, long runId, int monthIndex, CancellationToken cancellationToken)
    {
        var list = new List<CashflowSnapshotEntryDto>();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Name, CashflowType, Amount
            FROM Snapshots
            WHERE RunId = @runId AND MonthIndex = @monthIndex
            ORDER BY Sequence
            """;
        cmd.Parameters.AddWithValue("@runId", runId);
        cmd.Parameters.AddWithValue("@monthIndex", monthIndex);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var name = reader.GetString(0);
            var type = (CashflowType)reader.GetInt32(1);
            var amountStr = reader.GetString(2);
            var amount = decimal.Parse(amountStr, CultureInfo.InvariantCulture);
            list.Add(new CashflowSnapshotEntryDto { Name = name, CashflowType = type, Amount = amount });
        }
        return list;
    }

    private static readonly string[] SchemaStatements =
    [
        """
        CREATE TABLE IF NOT EXISTS Runs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CreatedAtUtc TEXT NOT NULL
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS MonthlyResults (
            RunId INTEGER NOT NULL,
            MonthIndex INTEGER NOT NULL,
            Age REAL NOT NULL,
            CashBalance TEXT NOT NULL,
            TotalAssets TEXT NOT NULL,
            PRIMARY KEY (RunId, MonthIndex),
            FOREIGN KEY (RunId) REFERENCES Runs(Id)
        )
        """,
        "CREATE INDEX IF NOT EXISTS IX_MonthlyResults_RunId ON MonthlyResults(RunId)",
        """
        CREATE TABLE IF NOT EXISTS Snapshots (
            RunId INTEGER NOT NULL,
            MonthIndex INTEGER NOT NULL,
            Sequence INTEGER NOT NULL,
            Name TEXT NOT NULL,
            CashflowType INTEGER NOT NULL,
            Amount TEXT NOT NULL,
            PRIMARY KEY (RunId, MonthIndex, Sequence),
            FOREIGN KEY (RunId) REFERENCES Runs(Id)
        )
        """,
        "CREATE INDEX IF NOT EXISTS IX_Snapshots_RunId_MonthIndex ON Snapshots(RunId, MonthIndex)"
    ];
}
