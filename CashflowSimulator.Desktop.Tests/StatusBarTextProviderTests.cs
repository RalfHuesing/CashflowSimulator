using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Main;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Tests für die Statusleisten-Anzeige in <see cref="MainShellViewModel"/> (nur Dateipfad oder "Bereit").
/// </summary>
public sealed class MainShellViewModelStatusBarTests
{
    [Fact]
    public void StatusBarDisplayText_WhenCurrentFilePathIsNull_ReturnsBereit()
    {
        var currentProject = new StubCurrentProjectService { CurrentFilePath = null };
        var vm = CreateMainShellViewModel(currentProject);
        Assert.Equal("Bereit", vm.StatusBarDisplayText);
    }

    [Fact]
    public void StatusBarDisplayText_WhenCurrentFilePathIsEmpty_ReturnsBereit()
    {
        var currentProject = new StubCurrentProjectService { CurrentFilePath = "" };
        var vm = CreateMainShellViewModel(currentProject);
        Assert.Equal("Bereit", vm.StatusBarDisplayText);
    }

    [Fact]
    public void StatusBarDisplayText_WhenCurrentFilePathIsSet_ReturnsFilePath()
    {
        const string path = @"C:\Szenarien\mein.json";
        var currentProject = new StubCurrentProjectService { CurrentFilePath = path };
        var vm = CreateMainShellViewModel(currentProject);
        Assert.Equal(path, vm.StatusBarDisplayText);
    }

    [Fact]
    public void StatusBarDisplayText_WhenContentIsIStatusBarContentProvider_ReturnsPrefixAndProviderText()
    {
        var currentProject = new StubCurrentProjectService { CurrentFilePath = null };
        var vm = CreateMainShellViewModel(currentProject);
        vm.CurrentContentViewModel = new StubStatusBarContentProvider("Fokus: X | Validierung: OK");
        Assert.Equal("Bereit | Fokus: X | Validierung: OK", vm.StatusBarDisplayText);
    }

    private sealed class StubStatusBarContentProvider : IStatusBarContentProvider
    {
        public StubStatusBarContentProvider(string statusBarText) => StatusBarText = statusBarText;
        public string StatusBarText { get; }
    }

    private static MainShellViewModel CreateMainShellViewModel(ICurrentProjectService currentProjectService)
    {
        var fileDialog = new StubFileDialogService();
        var storage = new StubStorageService();
        var theme = new StubThemeService();
        var nav = new NavigationViewModel();

        return new MainShellViewModel(
            fileDialog,
            storage,
            currentProjectService,
            theme,
            () => null!,
            () => null!,
            () => null!,
            nav,
            NullLogger.Instance);
    }

#pragma warning disable CS0067 // Event never used (stub for tests)
    private sealed class StubCurrentProjectService : ICurrentProjectService
    {
        public SimulationProjectDto? Current => null;
        public string? CurrentFilePath { get; set; }
        public void SetCurrent(SimulationProjectDto project, string? filePath = null) { }
        public void UpdateMeta(MetaDto meta) { }
        public void UpdateUiSettings(UiSettingsDto uiSettings) { }
        public void UpdateParameters(SimulationParametersDto parameters) { }
        public event EventHandler? ProjectChanged;
    }
#pragma warning restore CS0067

    private sealed class StubFileDialogService : IFileDialogService
    {
        public Task<string?> OpenAsync(FileDialogOptions options, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
        public Task<string?> SaveAsync(SaveFileDialogOptions options, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }

    private sealed class StubStorageService : IStorageService<SimulationProjectDto>
    {
        public Task<Result<SimulationProjectDto>> LoadAsync(string path, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> SaveAsync(string path, SimulationProjectDto data, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

#pragma warning disable CS0067
    private sealed class StubThemeService : IThemeService
    {
        public IReadOnlyList<ThemeOption> GetAvailableThemes() => [];
        public string GetDefaultThemeId() => "default";
        public void ApplyTheme(string? themeId) { }
        public event EventHandler? ThemeApplied;
    }
#pragma warning restore CS0067

    private sealed class NullLogger : ILogger<MainShellViewModel>
    {
        internal static readonly ILogger<MainShellViewModel> Instance = new NullLogger();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        public bool IsEnabled(LogLevel logLevel) => false;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
