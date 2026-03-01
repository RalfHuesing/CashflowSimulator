namespace CashflowSimulator.Desktop.ViewModels;

/// <summary>
/// Art der Statusmeldung für die Anzeige im Info-Panel (Farbe/Stil).
/// </summary>
public enum StatusType
{
    Info,
    Warning,
    Error,
    Success
}

/// <summary>
/// Ein Eintrag in der Status-Liste des Feature-Info-Panels.
/// </summary>
/// <param name="Message">Angezeigter Text.</param>
/// <param name="Type">Typ (Info, Warnung, Fehler, Erfolg) für die Darstellung.</param>
/// <param name="Id">Eindeutige Id zur Zuordnung beim zeitgesteuerten Entfernen.</param>
public sealed record StatusEntry(string Message, StatusType Type, Guid Id)
{
    public static StatusEntry Create(string message, StatusType type) =>
        new(message, type, Guid.NewGuid());
}
