using Avalonia.Controls;

namespace CashflowSimulator.Desktop.Common.Controls;

/// <summary>
/// Rahmen für Feature-Seiten: Content-Bereich links, GridSplitter, InfoPanel rechts (Fehler + Hilfetext).
/// DataContext ist das Feature-ViewModel (z. B. <see cref="ViewModels.ValidatingViewModelBase"/> mit ActiveHelpKey, ActiveHelpTitle, ActiveHelpDescription, ActiveHelpErrors).
/// </summary>
public partial class FeatureLayoutView : UserControl
{
    public FeatureLayoutView()
    {
        InitializeComponent();
    }
}
