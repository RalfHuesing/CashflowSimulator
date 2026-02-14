using Avalonia.Controls;
using Avalonia.Interactivity;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common.Behaviors;
using CashflowSimulator.Desktop.Features.Main;
using CashflowSimulator.Desktop.Services;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop;

public partial class MainWindow : Window
{
    private readonly AvaloniaFileDialogService _fileDialogService;
    private readonly IThemeService _themeService;
    private readonly ICurrentProjectService _currentProjectService;
/// <summary>
    /// Parameterloser Konstruktor für den Avalonia XAML-Loader / Previewer.
    /// Behebt Warnung AVLN3001. Wird zur Laufzeit NICHT verwendet (da DI genutzt wird).
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    public MainWindow()
    {
        InitializeComponent();
    }
#pragma warning restore CS8618

    public MainWindow(
        MainShellViewModel mainShellViewModel,
        AvaloniaFileDialogService fileDialogService,
        IThemeService themeService,
        ICurrentProjectService currentProjectService,
        ILogger<MainWindow> logger)
    {
        InitializeComponent();
        _fileDialogService = fileDialogService;
        _themeService = themeService;
        _currentProjectService = currentProjectService;
        DataContext = mainShellViewModel;

        logger.LogDebug("MainWindow initialisiert");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _fileDialogService.SetOwner(this);
        FocusHelpBehavior.Initialize(this);

        var themeId = _currentProjectService.Current?.UiSettings.SelectedThemeId;
        _themeService.ApplyTheme(string.IsNullOrWhiteSpace(themeId) ? _themeService.GetDefaultThemeId() : themeId);
    }
}
