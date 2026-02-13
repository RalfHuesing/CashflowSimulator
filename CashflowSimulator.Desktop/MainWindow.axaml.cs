using Avalonia.Controls;
using Avalonia.Interactivity;
using CashflowSimulator.Desktop.Features.Main;
using CashflowSimulator.Desktop.Services;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop;

public partial class MainWindow : Window
{
    private readonly AvaloniaFileDialogService _fileDialogService;

    public MainWindow(
        MainShellViewModel mainShellViewModel,
        AvaloniaFileDialogService fileDialogService,
        ILogger<MainWindow> logger)
    {
        InitializeComponent();
        _fileDialogService = fileDialogService;
        DataContext = mainShellViewModel;

        logger.LogDebug("MainWindow initialisiert");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _fileDialogService.SetOwner(this);
    }
}
