using CashflowSimulator.Infrastructure.Storage;
using Xunit;

namespace CashflowSimulator.Infrastructure.Tests;

public sealed class DraftsFolderCleanupTests
{
    [Fact]
    public void KeepNewest_WhenLessThanFiveFolders_LeavesAll()
    {
        using var temp = new TempDirectory();
        Directory.CreateDirectory(Path.Combine(temp.Path, "20250101-120000_1"));
        Directory.CreateDirectory(Path.Combine(temp.Path, "20250101-120001_2"));

        DraftsFolderCleanup.KeepNewest(temp.Path, 5);

        Assert.Equal(2, Directory.GetDirectories(temp.Path).Length);
    }

    [Fact]
    public void KeepNewest_WhenSixFolders_KeepsFiveNewest()
    {
        using var temp = new TempDirectory();
        for (var i = 1; i <= 6; i++)
            Directory.CreateDirectory(Path.Combine(temp.Path, $"2025010{i}-12000{i}_{i}"));

        DraftsFolderCleanup.KeepNewest(temp.Path, 5);

        var remaining = Directory.GetDirectories(temp.Path).Select(Path.GetFileName).OrderBy(x => x).ToList();
        Assert.Equal(5, remaining.Count);
        Assert.Contains("20250106-120006_6", remaining);
        Assert.Contains("20250105-120005_5", remaining);
        Assert.DoesNotContain(remaining, x => x == "20250101-120001_1");
    }

    [Fact]
    public void KeepNewest_WhenPathDoesNotExist_DoesNotThrow()
    {
        var nonExistent = Path.Combine(Path.GetTempPath(), "CashflowSimulator_Test_NonExistent_" + Guid.NewGuid().ToString("N"));
        DraftsFolderCleanup.KeepNewest(nonExistent, 5);
    }

    [Fact]
    public void KeepNewest_WhenSameTimestamp_SortsByFullNameDescending_KeepsTwoNewest()
    {
        using var temp = new TempDirectory();
        var baseName = "20250101-120000";
        Directory.CreateDirectory(Path.Combine(temp.Path, baseName + "_1"));
        Directory.CreateDirectory(Path.Combine(temp.Path, baseName + "_2"));
        Directory.CreateDirectory(Path.Combine(temp.Path, baseName + "_3"));

        DraftsFolderCleanup.KeepNewest(temp.Path, 2);

        var remaining = Directory.GetDirectories(temp.Path).Select(Path.GetFileName).OrderBy(x => x).ToList();
        Assert.Equal(2, remaining.Count);
        Assert.Contains(baseName + "_3", remaining);
        Assert.Contains(baseName + "_2", remaining);
        Assert.DoesNotContain(remaining, x => x == baseName + "_1");
    }

    [Fact]
    public void KeepNewest_WhenExactlyKeepCountFolders_LeavesAll()
    {
        using var temp = new TempDirectory();
        for (var i = 1; i <= 5; i++)
            Directory.CreateDirectory(Path.Combine(temp.Path, $"2025010{i}-12000{i}_{i}"));

        DraftsFolderCleanup.KeepNewest(temp.Path, 5);

        Assert.Equal(5, Directory.GetDirectories(temp.Path).Length);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CashflowSimulator_CleanupTest_" + Guid.NewGuid().ToString("N"));

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                    Directory.Delete(Path, recursive: true);
            }
            catch { /* ignore */ }
        }
    }
}
