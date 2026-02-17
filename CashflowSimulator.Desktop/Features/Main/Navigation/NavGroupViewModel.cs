using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Eine gruppierte Sektion in der Navigation mit auf- und zuklappbaren Einträgen.
/// Standard: zugeklappt (IsExpanded = false).
/// </summary>
public partial class NavGroupViewModel : ObservableObject
{
    /// <summary>
    /// Anzeigename der Gruppe (z. B. "Stammdaten", "Profile").
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gibt an, ob die Gruppe aufgeklappt ist und ihre Einträge anzeigt.
    /// Standard: false (zugeklappt).
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;

    /// <summary>
    /// Die Navigationspunkte innerhalb dieser Gruppe.
    /// </summary>
    public ObservableCollection<NavItemViewModel> Items { get; } = [];
}
