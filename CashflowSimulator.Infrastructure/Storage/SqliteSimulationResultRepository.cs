using System.Globalization;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using Microsoft.Data.Sqlite;

namespace CashflowSimulator.Infrastructure.Storage;

/// <summary>
/// SQLite-Repository für Simulationsergebnisse. Eine frische, leere DB pro Run im Temp-Verzeichnis.
/// Speicherort: %Temp%\CashflowSimulator\ (OS-unabhängig). Vor jedem StartRun werden nur eigene Dateien (run_*.db) gelöscht.
/// </summary>
public sealed class SqliteSimulationResultRepository : ISimulationResultRepository
{
    private static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), "CashflowSimulator");
    private const string FilePrefix = "run_";
    private const string FileSuffix = ".db";

    /// <inheritdoc />
    public long StartRun()
    {
        EnsureTempDirectoryExists();
        DeleteOwnDatabaseFiles();

        long runId;
        using (var memoryConnection = new SqliteConnection("Data Source=:memory:"))
        {
            memoryConnection.Open();
            CreateSchema(memoryConnection);
            using (var cmd = memoryConnection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Runs (CreatedAtUtc) VALUES (datetime('now')); SELECT last_insert_rowid();";
                runId = (long)cmd.ExecuteScalar()!;
            }

            var finalPath = Path.Combine(TempDirectory, $"{FilePrefix}{runId}{FileSuffix}");
            using (var fileConnection = new SqliteConnection($"Data Source={finalPath}"))
            {
                memoryConnection.BackupDatabase(fileConnection);
            }
        }

        return runId;
    }

    /// <inheritdoc />
    public void WriteMonthlyResult(long runId, MonthlyResultDto entry)
    {
        var path = GetDbPath(runId);
        if (!File.Exists(path))
            throw new InvalidOperationException($"Run {runId} nicht gefunden (DB-Datei fehlt).");

        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        using (var tx = connection.BeginTransaction())
        {
            using (var cmd = connection.CreateCommand())
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
                cmd.ExecuteNonQuery();
            }

            var snapshots = entry.CashflowSnapshots ?? [];
            for (var i = 0; i < snapshots.Count; i++)
            {
                var s = snapshots[i];
                using var cmd = connection.CreateCommand();
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
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }

    /// <inheritdoc />
    public void CompleteRun(long runId)
    {
        // Kein offener Connection-Handle; optional später: Transaktion flushen o. Ä.
    }

    /// <inheritdoc />
    public IReadOnlyList<MonthlyResultDto> GetMonthlyResults(long runId)
    {
        var path = GetDbPath(runId);
        if (!File.Exists(path))
            return [];

        var results = new List<MonthlyResultDto>();
        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT MonthIndex, Age, CashBalance, TotalAssets
            FROM MonthlyResults
            WHERE RunId = @runId
            ORDER BY MonthIndex
            """;
        cmd.Parameters.AddWithValue("@runId", runId);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var monthIndex = reader.GetInt32(0);
            var age = reader.GetDouble(1);
            var cashStr = reader.GetString(2);
            var assetsStr = reader.GetString(3);
            var cash = decimal.Parse(cashStr, CultureInfo.InvariantCulture);
            var assets = decimal.Parse(assetsStr, CultureInfo.InvariantCulture);
            var snapshots = GetSnapshots(connection, runId, monthIndex);
            results.Add(new MonthlyResultDto
            {
                MonthIndex = monthIndex,
                Age = age,
                CashBalance = cash,
                TotalAssets = assets,
                CashflowSnapshots = snapshots
            });
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

    private static void CreateSchema(SqliteConnection connection)
    {
        foreach (var sql in SchemaStatements)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }

    private static List<CashflowSnapshotEntryDto> GetSnapshots(SqliteConnection connection, long runId, int monthIndex)
    {
        var list = new List<CashflowSnapshotEntryDto>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Name, CashflowType, Amount
            FROM Snapshots
            WHERE RunId = @runId AND MonthIndex = @monthIndex
            ORDER BY Sequence
            """;
        cmd.Parameters.AddWithValue("@runId", runId);
        cmd.Parameters.AddWithValue("@monthIndex", monthIndex);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
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
