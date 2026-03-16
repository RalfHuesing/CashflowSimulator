using System;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Konfiguriert die Navigationsstruktur der Anwendung.
/// </summary>
public interface INavigationConfiguration
{
    /// <summary>
    /// Baut die Navigationsgruppen und -elemente auf.
    /// </summary>
    /// <param name="navigation">Das NavigationViewModel, das konfiguriert werden soll.</param>
    /// <param name="navigateAction">Die Aktion zum Ausführen der Navigation (TargetViewType, optional Parameter).</param>
    /// <param name="canNavigatePredicate">Ein Delegate, das prüft, ob die Navigation aktuell erlaubt ist.</param>
    void Configure(NavigationViewModel navigation, Func<Type, object[]?, System.Threading.Tasks.Task> navigateAction, Func<bool> canNavigatePredicate);
}
