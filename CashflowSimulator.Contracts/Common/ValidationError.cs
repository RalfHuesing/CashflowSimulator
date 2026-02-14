namespace CashflowSimulator.Contracts.Common;

/// <summary>
/// Ein einzelner Validierungsfehler (Property und Meldung).
/// Wird von der Validation-Schicht erzeugt und in der UI angezeigt.
/// </summary>
/// <param name="PropertyName">Feld- oder Eigenschaftsname (kann leer sein für Objektfehler).</param>
/// <param name="Message">Fehlermeldung für den Nutzer (Deutsch).</param>
public readonly record struct ValidationError(string PropertyName, string Message);
