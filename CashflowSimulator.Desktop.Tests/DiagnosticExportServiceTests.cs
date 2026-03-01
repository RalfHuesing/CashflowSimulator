using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class DiagnosticExportServiceTests
{
    [Fact]
    public async Task ExportAsync_WhenLastRunFolderPathIsNull_DoesNotWriteFile()
    {
        var projectService = new CurrentProjectService();
        // LastRunFolderPath bleibt null (kein SetLastRunId mit Pfad aufgerufen)
        var service = new DiagnosticExportService(projectService, NullLogger<DiagnosticExportService>.Instance);
        var source = new TestDiagnosticExport("test.json", new { A = 1 });

        await service.ExportAsync(source);

        Assert.False(File.Exists(Path.Combine(Path.GetTempPath(), "Diagnostics", "test.json")));
    }

    [Fact]
    public async Task ExportAsync_WhenLastRunFolderPathIsSet_WritesJsonToDiagnosticsSubfolder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "CashflowSimulatorDiagnosticTests", Guid.NewGuid().ToString("N"));
        try
        {
            var projectService = new CurrentProjectService();
            projectService.SetLastRunId(42, tempDir);
            var service = new DiagnosticExportService(projectService, NullLogger<DiagnosticExportService>.Instance);
            var source = new TestDiagnosticExport("snapshot.json", new { RunId = 42, Label = "Test" });

            await service.ExportAsync(source);

            var expectedPath = Path.Combine(tempDir, "Diagnostics", "snapshot.json");
            Assert.True(File.Exists(expectedPath));
            var json = await File.ReadAllTextAsync(expectedPath);
            Assert.Contains("runId", json);
            Assert.Contains("42", json);
            Assert.Contains("label", json);
            Assert.Contains("Test", json);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_WhenFileNameMissingJson_AppendsJsonExtension()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "CashflowSimulatorDiagnosticTests", Guid.NewGuid().ToString("N"));
        try
        {
            var projectService = new CurrentProjectService();
            projectService.SetLastRunId(1, tempDir);
            var service = new DiagnosticExportService(projectService, NullLogger<DiagnosticExportService>.Instance);
            var source = new TestDiagnosticExport("analysis-dashboard", new { X = 1 });

            await service.ExportAsync(source);

            var expectedPath = Path.Combine(tempDir, "Diagnostics", "analysis-dashboard.json");
            Assert.True(File.Exists(expectedPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_WhenCalledConcurrently_NoIOException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "CashflowSimulatorDiagnosticTests", Guid.NewGuid().ToString("N"));
        try
        {
            var projectService = new CurrentProjectService();
            projectService.SetLastRunId(1, tempDir);
            var logger = NullLogger<DiagnosticExportService>.Instance;
            var service = new DiagnosticExportService(projectService, logger);
            var source1 = new TestDiagnosticExport("a.json", new { Id = 1 });
            var source2 = new TestDiagnosticExport("b.json", new { Id = 2 });

            var t1 = service.ExportAsync(source1);
            var t2 = service.ExportAsync(source2);
            await Task.WhenAll(t1, t2);

            Assert.True(File.Exists(Path.Combine(tempDir, "Diagnostics", "a.json")));
            Assert.True(File.Exists(Path.Combine(tempDir, "Diagnostics", "b.json")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    private sealed class TestDiagnosticExport : IDiagnosticExport
    {
        private readonly string _fileName;
        private readonly object _data;

        public TestDiagnosticExport(string fileName, object data)
        {
            _fileName = fileName;
            _data = data;
        }

        public object GetExportData() => _data;
        public string ExportFileName => _fileName;
    }
}
