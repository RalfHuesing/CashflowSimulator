using System.Globalization;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using Microsoft.Data.Sqlite;

namespace CashflowSimulator.Infrastructure.Storage;

/// <summary>
/// SQLite-Repository für Simulationsergebnisse. Eine frische, leere DB pro Run im Temp-Verzeichnis.
/// Speicherort: %Temp%\CashflowSimulator\ (OS-unabhängig). Vor jedem StartRunAsync werden nur eigene Dateien (run_*.db) gelöscht.
/// Nutzt eine Connection/Transaktion pro Batch (WriteMonthlyResultsAsync) zur Vermeidung von N+1.
/// </summary>
public sealed class SqliteSimulationResultRepository : ISimulationResultRepository
{
    private static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), "CashflowSimulator");
    private const string FilePrefix = "run_";
    private const string FileSuffix = ".db";

    /// <inheritdoc />
    public async Task<long> StartRunAsync(CancellationToken cancellationToken = default)
    {
        EnsureTempDirectoryExists();
        DeleteOwnDatabaseFiles();

        await using var memoryConnection = new SqliteConnection("Data Source=:memory:");
        await memoryConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await CreateSchemaAsync(memoryConnection, cancellationToken).ConfigureAwait(false);

        long runId;
        await using (var cmd = memoryConnection.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO Runs (CreatedAtUtc) VALUES (datetime('now')); SELECT last_insert_rowid();";
            var scalar = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            runId = Convert.ToInt64(scalar, CultureInfo.InvariantCulture);
        }

        var finalPath = Path.Combine(TempDirectory, $"{FilePrefix}{runId}{FileSuffix}");
        await using (var fileConnection = new SqliteConnection($"Data Source={finalPath}"))
        {
            await fileConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await Task.Run(() => memoryConnection.BackupDatabase(fileConnection), cancellationToken).ConfigureAwait(false);
        }

        return runId;
    }

    /// <inheritdoc />
    public async Task WriteMonthlyResultsAsync(long runId, IEnumerable<MonthlyResultDto> entries, CancellationToken cancellationToken = default)
    {
        var path = GetDbPath(runId);
        if (!File.Exists(path))
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
        // Kein offener Connection-Handle; optional später: Transaktion flushen o. Ä.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonthlyResultDto>> GetMonthlyResultsAsync(long runId, CancellationToken cancellationToken = default)
    {
        var path = GetDbPath(runId);
        if (!File.Exists(path))
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

    private static void EnsureTempDirectoryExists()
    {
        if (!Directory.Exists(TempDirectory))
            Directory.CreateDirectory(TempDirectory);
    }

    /// <summary>
    /// Löscht nur Dateien, die wir anlegen (run_*.db), aus dem Temp-Verzeichnis.
    /// </summary>
    private static void DeleteOwnDatabaseFiles()
    {
        if (!Directory.Exists(TempDirectory))
            return;
        foreach (var file in Directory.EnumerateFiles(TempDirectory, $"{FilePrefix}*{FileSuffix}"))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Ignorieren (z. B. von anderem Prozess geöffnet)
            }
        }
    }

    private static string GetDbPath(long runId) => Path.Combine(TempDirectory, $"{FilePrefix}{runId}{FileSuffix}");

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
