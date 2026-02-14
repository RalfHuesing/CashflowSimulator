using Avalonia.Controls;

namespace CashflowSimulator.Desktop.Common.Controls;

/// <summary>
/// Rahmen für Feature-Seiten: Content-Bereich links, GridSplitter, InfoPanel rechts (Fehler + Hilfetext).
/// Erbt von ContentControl; das Layout (Grid + InfoPanel) kommt aus dem Style-Template in Default.axaml.
/// So wird der XAML-Kind-Inhalt nur im ContentPresenter angezeigt und ersetzt nicht das gesamte Layout.
/// DataContext ist das Feature-ViewModel (z. B. ValidatingViewModelBase).
/// </summary>
public partial class FeatureLayoutView : ContentControl
{
    public FeatureLayoutView()
    {
        InitializeComponent();
    }
}
