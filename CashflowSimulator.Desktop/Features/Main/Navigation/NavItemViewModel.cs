using System.Windows.Input;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Ein Eintrag in der linken Navigation (Icon + Text, Command beim Klick).
/// </summary>
public class NavItemViewModel
{
    public string DisplayName { get; init; } = string.Empty;
    /// <summary>
    /// Optional: Ressourcen-Schlüssel oder Symbol-Name für ein Icon (z. B. FluentIcons).
    /// </summary>
    public string? IconKey { get; init; }
    public ICommand Command { get; init; } = null!;
}
