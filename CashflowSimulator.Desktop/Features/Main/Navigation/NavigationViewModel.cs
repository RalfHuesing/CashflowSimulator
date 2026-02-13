using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Hält die Liste der Navigationselemente.
/// Wird als Singleton oder im Scope der MainShell verwendet.
/// </summary>
public partial class NavigationViewModel : ObservableObject
{
    /// <summary>
    /// Liste der sichtbaren Navigationspunkte.
    /// </summary>
    public ObservableCollection<NavItemViewModel> Items { get; } = [];

    /// <summary>
    /// Konstruktor.
    /// Hier könnten später statische Navigationspunkte (wie "Home") initialisiert werden,
    /// aktuell erfolgt die Befüllung dynamisch über die MainShell.
    /// </summary>
    public NavigationViewModel()
    {
        // Leerer Konstruktor für DI.
        // Items werden zur Laufzeit von der Shell hinzugefügt (z.B. je nach geladenem Projekt).
    }
}