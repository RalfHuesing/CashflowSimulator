using System.Collections.ObjectModel;
using CashflowSimulator.Contracts.Common;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Implementierung von <see cref="IValidationMessageService"/>.
/// Hält eine beobachtbare Liste von Meldungen für die Shell.
/// </summary>
public sealed class ValidationMessageService : IValidationMessageService
{
    public ObservableCollection<ValidationMessageEntry> Messages { get; } = new();

    public void SetErrors(string source, IReadOnlyList<ValidationError> errors)
    {
        if (string.IsNullOrEmpty(source))
            return;

        ClearSource(source);
        if (errors is null || errors.Count == 0)
            return;

        foreach (var err in errors)
            Messages.Add(new ValidationMessageEntry(source, err.PropertyName, err.Message));
    }

    public void ClearSource(string source)
    {
        if (string.IsNullOrEmpty(source))
            return;

        for (var i = Messages.Count - 1; i >= 0; i--)
        {
            if (Messages[i].Source == source)
                Messages.RemoveAt(i);
        }
    }
}
