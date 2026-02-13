using System.Collections.ObjectModel;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// ViewModel für die linke Navigationsleiste (Icon + Text).
/// </summary>
public class NavigationViewModel
{
    public ObservableCollection<NavItemViewModel> Items { get; } = new();
}
