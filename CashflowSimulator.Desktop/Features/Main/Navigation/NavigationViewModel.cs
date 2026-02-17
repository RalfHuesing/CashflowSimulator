using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Hält die gruppierte Navigation (auf-/zuklappbare Sektionen).
/// Wird als Singleton oder im Scope der MainShell verwendet.
/// </summary>
public partial class NavigationViewModel : ObservableObject
{
    /// <summary>
    /// Gruppierte Navigationspunkte. Jede Gruppe kann zu- und aufgeklappt werden.
    /// </summary>
    public ObservableCollection<NavGroupViewModel> Groups { get; } = [];

    /// <summary>
    /// Liefert alle Nav-Items in fester Reihenfolge (gruppenweise).
    /// Wird für die Active-State-Steuerung nach Index benötigt.
    /// </summary>
    public IReadOnlyList<NavItemViewModel> GetAllItems()
    {
        var list = new List<NavItemViewModel>();
        foreach (var group in Groups)
        {
            foreach (var item in group.Items)
                list.Add(item);
        }
        return list;
    }
}
