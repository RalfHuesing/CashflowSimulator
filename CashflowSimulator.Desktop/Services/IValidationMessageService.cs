using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Zentrale Anzeige von Validierungsfehlern in der Shell (Option B).
/// Feature-ViewModels melden Fehler hier; die Shell bindet an <see cref="Messages"/>.
/// </summary>
public interface IValidationMessageService
{
    /// <summary>
    /// Alle aktuellen Meldungen (gebunden an die Shell).
    /// </summary>
    ObservableCollection<ValidationMessageEntry> Messages { get; }

    /// <summary>
    /// Fehler für eine Quelle (z. B. "Eckdaten") setzen; bestehende Meldungen dieser Quelle werden ersetzt.
    /// </summary>
    void SetErrors(string source, IReadOnlyList<ValidationError> errors);

    /// <summary>
    /// Alle Meldungen einer Quelle entfernen (z. B. nach erfolgreichem Apply).
    /// </summary>
    void ClearSource(string source);
}
