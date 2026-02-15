using System.Text.Json;
using CashflowSimulator.Infrastructure.Storage;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Unit-Tests für <see cref="JsonFileStorageService{T}"/>: LoadAsync/SaveAsync,
/// leere Pfade, Datei nicht gefunden, ungültiges JSON, null-Daten, Roundtrip.
/// </summary>
public sealed class JsonFileStorageServiceTests
{
    private sealed class TestDto
    {
        public string Name { get; init; } = "";
        public int Value { get; init; }
    }

    private static JsonFileStorageService<TestDto> CreateSut(JsonSerializerOptions? options = null) =>
        new JsonFileStorageService<TestDto>(options);

    [Fact]
    public async Task LoadAsync_EmptyPath_ReturnsFailure()
    {
        var sut = CreateSut();

        var result = await sut.LoadAsync("");

        Assert.False(result.IsSuccess);
        Assert.Contains("leer", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_WhitespacePath_ReturnsFailure()
    {
        var sut = CreateSut();

        var result = await sut.LoadAsync("   ");

        Assert.False(result.IsSuccess);
        Assert.Contains("leer", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ReturnsFailure()
    {
        var sut = CreateSut();
        var path = Path.Combine(Path.GetTempPath(), $"CashflowSimulator_Test_{Guid.NewGuid():N}.json");

        var result = await sut.LoadAsync(path);

        Assert.False(result.IsSuccess);
        Assert.Contains("nicht gefunden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_ReturnsFailure()
    {
        var sut = CreateSut();
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "not valid json {");

            var result = await sut.LoadAsync(path);

            Assert.False(result.IsSuccess);
            Assert.Contains("JSON", result.Error!, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ReturnsFailure()
    {
        var sut = CreateSut();
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "null");

            var result = await sut.LoadAsync(path);

            Assert.False(result.IsSuccess);
            Assert.Contains("gültigen Daten", result.Error!, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_ValidJson_ReturnsSuccessAndValue()
    {
        var sut = CreateSut();
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """{"name":"Test","value":42}""");

            var result = await sut.LoadAsync(path);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Test", result.Value!.Name);
            Assert.Equal(42, result.Value.Value);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task SaveAsync_EmptyPath_ReturnsFailure()
    {
        var sut = CreateSut();
        var data = new TestDto { Name = "A", Value = 1 };

        var result = await sut.SaveAsync("", data);

        Assert.False(result.IsSuccess);
        Assert.Contains("leer", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveAsync_NullData_ReturnsFailure()
    {
        var sut = CreateSut();
        var path = Path.GetTempFileName();
        try
        {
            var result = await sut.SaveAsync(path, null!);

            Assert.False(result.IsSuccess);
            Assert.Contains("Daten", result.Error!, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task SaveAsync_ValidData_WritesFileAndReturnsSuccess()
    {
        var sut = CreateSut();
        var dir = Path.Combine(Path.GetTempPath(), "CashflowSimulator_Tests");
        var path = Path.Combine(dir, $"test_{Guid.NewGuid():N}.json");
        var data = new TestDto { Name = "Roundtrip", Value = 99 };
        try
        {
            var result = await sut.SaveAsync(path, data);

            Assert.True(result.IsSuccess);
            Assert.True(File.Exists(path));
            var json = await File.ReadAllTextAsync(path);
            Assert.Contains("Roundtrip", json);
            Assert.Contains("99", json);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
            if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                Directory.Delete(dir);
        }
    }

    [Fact]
    public async Task SaveAsyncThenLoadAsync_Roundtrip_PreservesData()
    {
        var sut = CreateSut();
        var path = Path.GetTempFileName();
        var original = new TestDto { Name = "Roundtrip", Value = 123 };
        try
        {
            var saveResult = await sut.SaveAsync(path, original);
            Assert.True(saveResult.IsSuccess);

            var loadResult = await sut.LoadAsync(path);
            Assert.True(loadResult.IsSuccess);
            Assert.Equal(original.Name, loadResult.Value!.Name);
            Assert.Equal(original.Value, loadResult.Value.Value);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
