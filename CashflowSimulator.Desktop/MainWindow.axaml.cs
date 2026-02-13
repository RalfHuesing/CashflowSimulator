using Avalonia.Controls;
using Avalonia.Interactivity;
using CashflowSimulator.Desktop.Features.Main;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Services;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop;

public partial class MainWindow : Window
{
    private readonly AvaloniaFileDialogService _fileDialogService;
    private readonly MetaEditDialogService _metaEditDialogService;

    public MainWindow(
        MainShellViewModel mainShellViewModel,
        AvaloniaFileDialogService fileDialogService,
        MetaEditDialogService metaEditDialogService,
        ILogger<MainWindow> logger)
    {
        InitializeComponent();
        _fileDialogService = fileDialogService;
        _metaEditDialogService = metaEditDialogService;
        DataContext = mainShellViewModel;

        logger.LogDebug("MainWindow initialisiert");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _fileDialogService.SetOwner(this);
        _metaEditDialogService.SetOwner(this);
    }
}
