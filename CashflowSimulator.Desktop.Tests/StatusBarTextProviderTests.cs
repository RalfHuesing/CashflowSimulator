using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Unit-Tests für die zentrale Statusleisten-Text-Logik.
/// </summary>
public sealed class StatusBarTextProviderTests
{
    [Fact]
    public void GetStatusText_WhenNoErrorsAndNoFilePath_ReturnsBereit()
    {
        var result = StatusBarTextProvider.GetStatusText(false, 0, null);
        Assert.Equal("Bereit", result);
    }

    [Fact]
    public void GetStatusText_WhenNoErrorsAndEmptyFilePath_ReturnsBereit()
    {
        var result = StatusBarTextProvider.GetStatusText(false, 0, "");
        Assert.Equal("Bereit", result);
    }

    [Fact]
    public void GetStatusText_WhenNoErrorsAndFilePathSet_ReturnsFilePath()
    {
        const string path = @"C:\Szenarien\mein.json";
        var result = StatusBarTextProvider.GetStatusText(false, 0, path);
        Assert.Equal(path, result);
    }

    [Fact]
    public void GetStatusText_WhenOneError_ReturnsOneFehlerGefunden()
    {
        var result = StatusBarTextProvider.GetStatusText(true, 1, null);
        Assert.Equal("1 Fehler gefunden", result);
    }

    [Fact]
    public void GetStatusText_WhenMultipleErrors_ReturnsCountFehlerGefunden()
    {
        var result = StatusBarTextProvider.GetStatusText(true, 10, @"C:\x.json");
        Assert.Equal("10 Fehler gefunden", result);
    }

    [Fact]
    public void GetStatusText_WhenErrors_IgnoresFilePath()
    {
        var result = StatusBarTextProvider.GetStatusText(true, 2, @"C:\wird-ignoriert.json");
        Assert.Equal("2 Fehler gefunden", result);
    }

    [Fact]
    public void GetStatusBarDisplayText_WhenOneErrorAndFirstMessageSet_ReturnsFirstMessageText()
    {
        const string msg = "Eckdaten · Renteneintritt: Rente muss nach Geburt liegen.";
        var result = StatusBarTextProvider.GetStatusBarDisplayText(true, 1, msg, null);
        Assert.Equal(msg, result);
    }

    [Fact]
    public void GetStatusBarDisplayText_WhenOneErrorAndFirstMessageNull_ReturnsOneFehlerGefunden()
    {
        var result = StatusBarTextProvider.GetStatusBarDisplayText(true, 1, null, null);
        Assert.Equal("1 Fehler gefunden", result);
    }

    [Fact]
    public void GetStatusBarDisplayText_WhenNoErrors_ReturnsBereit()
    {
        var result = StatusBarTextProvider.GetStatusBarDisplayText(false, 0, null, null);
        Assert.Equal("Bereit", result);
    }

    [Fact]
    public void GetStatusBarDisplayText_WhenMultipleErrors_ReturnsCountFehlerGefunden()
    {
        var result = StatusBarTextProvider.GetStatusBarDisplayText(true, 5, "irrelevant", @"C:\x.json");
        Assert.Equal("5 Fehler gefunden", result);
    }
}
