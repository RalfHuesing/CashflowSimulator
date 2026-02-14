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
