namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Stellt den Text für die Statusleiste der Shell bereit (Fokus, Validierungsstatus).
/// Wird von Feature-ViewModels implementiert, die im Content-Bereich angezeigt werden.
/// </summary>
public interface IStatusBarContentProvider
{
    /// <summary>
    /// Vollständiger Text für die Statuszeile (z. B. "Bereit | Fokus: Start-Kapital | Validierung: 1 Fehler").
    /// </summary>
    string StatusBarText { get; }
}
