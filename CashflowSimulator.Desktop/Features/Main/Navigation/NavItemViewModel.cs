using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Ein Eintrag in der linken Navigation (Icon + Text, Command beim Klick).
/// </summary>
public partial class NavItemViewModel : ObservableObject
{
    public string DisplayName { get; init; } = string.Empty;
    /// <summary>
    /// Optional: Ressourcen-Schlüssel oder Symbol-Name für ein Icon (z. B. FluentIcons).
    /// </summary>
    public string? IconKey { get; init; }
    public ICommand Command { get; init; } = null!;

    /// <summary>
    /// Ob dieser Eintrag aktuell ausgewählt/aktiv ist (für Hervorhebung in der Nav).
    /// </summary>
    [ObservableProperty]
    private bool _isActive;
}
