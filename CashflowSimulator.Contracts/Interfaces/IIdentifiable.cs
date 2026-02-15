namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Interface für Entitäten mit einer eindeutigen ID.
/// Ermöglicht generische CRUD-Operationen in ViewModels.
/// </summary>
public interface IIdentifiable
{
    /// <summary>
    /// Eindeutige ID der Entität (typischerweise Guid-String).
    /// </summary>
    string Id { get; }
}
