using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow(ILogger<MainWindow> logger)
        {
            InitializeComponent();
            logger.LogDebug("MainWindow initialisiert");
        }
    }
}