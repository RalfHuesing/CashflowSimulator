using System.Collections;
using System.ComponentModel;
using CashflowSimulator.Contracts.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.ViewModels;

/// <summary>
/// Basis für Edit-ViewModels mit Avalonia-Standard-Validierung (INotifyDataErrorInfo).
/// Fehler werden ausschließlich von außen gesetzt (z. B. aus FluentValidation); keine DataAnnotations.
/// Unterstützt Property-Mapping (DTO → VM) und Objekt-Fehler (FormLevelErrors).
/// </summary>
public abstract class ValidatingViewModelBase : ObservableObject, INotifyDataErrorInfo
{
    /// <summary>
    /// Synthetische Property für Fehler ohne Property (z. B. RuleFor(x => x) in FluentValidation).
    /// In der View anzeigbar z. B. über Bindung an <see cref="FormLevelErrors"/>.
    /// </summary>
    public const string FormLevelErrorsKey = "FormLevelErrors";

    private readonly Dictionary<string, List<string>> _errors = new();
    private readonly Debouncer _debouncer = new();

    /// <summary>
    /// Für Bindung in der View: Objekt-Fehler (kein einzelnes Feld zugeordnet).
    /// </summary>
    public IEnumerable<string> FormLevelErrors =>
        _errors.TryGetValue(FormLevelErrorsKey, out var list) ? list : [];

    /// <summary>
    /// True, wenn mindestens ein Objekt-Fehler (FormLevelErrors) vorliegt.
    /// </summary>
    public bool HasFormLevelErrors => _errors.ContainsKey(FormLevelErrorsKey);

    /// <inheritdoc />
    public bool HasErrors => _errors.Count > 0;

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Setzt die Validierungsfehler; mappt DTO-Property-Namen auf VM-Property-Namen.
    /// Leere oder unbekannte PropertyNames landen in <see cref="FormLevelErrorsKey"/>.
    /// </summary>
    /// <param name="errors">Fehler von ValidationRunner / FluentValidation.</param>
    /// <param name="dtoToVmPropertyMap">Optional: DTO-PropertyName → VM-PropertyName (z. B. DateOfBirth → BirthDate).</param>
    protected void SetValidationErrors(
        IReadOnlyList<ValidationError> errors,
        IReadOnlyDictionary<string, string>? dtoToVmPropertyMap = null)
    {
        var previousProperties = new HashSet<string>(_errors.Keys);
        _errors.Clear();

        if (errors is not null && errors.Count > 0)
        {
            foreach (var err in errors)
            {
                var vmProperty = string.IsNullOrEmpty(err.PropertyName)
                    ? FormLevelErrorsKey
                    : (dtoToVmPropertyMap?.TryGetValue(err.PropertyName, out var mapped) == true ? mapped : err.PropertyName);

                if (!_errors.TryGetValue(vmProperty, out var list))
                {
                    list = [];
                    _errors[vmProperty] = list;
                }
                list.Add(err.Message);
            }
        }

        var changed = new HashSet<string>(previousProperties);
        foreach (var key in _errors.Keys)
            changed.Add(key);
        foreach (var propertyName in changed)
            OnErrorsChanged(propertyName);

        OnPropertyChanged(nameof(FormLevelErrors));
        OnPropertyChanged(nameof(HasFormLevelErrors));
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// Löscht alle Validierungsfehler.
    /// </summary>
    protected void ClearValidationErrors()
    {
        if (_errors.Count == 0)
            return;

        var previous = new HashSet<string>(_errors.Keys);
        _errors.Clear();
        foreach (var propertyName in previous)
            OnErrorsChanged(propertyName);
        OnPropertyChanged(nameof(FormLevelErrors));
        OnPropertyChanged(nameof(HasFormLevelErrors));
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <inheritdoc />
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(list => list).ToList();
        return _errors.TryGetValue(propertyName, out var list) ? list : [];
    }

    /// <summary>
    /// Führt <paramref name="action"/> nach <paramref name="delayMs"/> aus; bei erneutem Aufruf wird der Timer zurückgesetzt (Debounce).
    /// Die Ausführung erfolgt auf dem UI-Thread (Avalonia).
    /// </summary>
    protected void ScheduleDebounced(int delayMs, Action action)
    {
        _debouncer.Run(delayMs, action);
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Debouncer: führt eine Aktion nach Verzögerung aus; bei erneutem Run wird die vorherige Ausführung abgebrochen.
    /// </summary>
    private sealed class Debouncer
    {
        private CancellationTokenSource? _cts;

        internal void Run(int delayMs, Action action)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delayMs, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (!token.IsCancellationRequested)
                        action();
                });
            }, token);
        }
    }
}
