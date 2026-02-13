using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Ein einzelner Eintrag in der Navigationsleiste.
/// Unterstützt nun native Vektor-Icons via FluentIcons.
/// </summary>
public partial class NavItemViewModel : ObservableObject
{
    /// <summary>
    /// Der Anzeigetext (z.B. "Dashboard", "Stammdaten").
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Das Icon-Symbol aus dem Fluent-System.
    /// Default ist ein Platzhalter, sollte bei Initialisierung gesetzt werden.
    /// </summary>
    public Symbol Icon { get; init; } = Symbol.Document;

    /// <summary>
    /// Der Command, der ausgeführt wird (Navigation oder Aktion).
    /// </summary>
    public required ICommand Command { get; init; }

    /// <summary>
    /// Steuert den visuellen "Active State" (z.B. farbiger Balken links).
    /// </summary>
    [ObservableProperty]
    private bool _isActive;
}