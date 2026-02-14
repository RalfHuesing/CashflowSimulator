using System.Collections;
using System.ComponentModel;
using CashflowSimulator.Contracts.Common;
using CashflowSimulator.Contracts.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.ViewModels;

/// <summary>
/// Basis für Edit-ViewModels mit Avalonia-Standard-Validierung (INotifyDataErrorInfo).
/// Fehler werden ausschließlich von außen gesetzt (z. B. aus FluentValidation); keine DataAnnotations.
/// Unterstützt Property-Mapping (DTO → VM), Objekt-Fehler (FormLevelErrors) und Help-Panel.
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
    private readonly IHelpProvider? _helpProvider;

    private string? _activeHelpKey;
    private string? _pageHelpKey;
    private string? _activeHelpTitle;
    private string? _activeHelpDescription;

    /// <summary>
    /// Konstruktor für abgeleitete ViewModels. Optional kann ein <see cref="IHelpProvider"/> übergeben werden,
    /// um <see cref="ActiveHelpTitle"/> und <see cref="ActiveHelpDescription"/> automatisch zu befüllen.
    /// </summary>
    protected ValidatingViewModelBase(IHelpProvider? helpProvider = null)
    {
        _helpProvider = helpProvider;
    }

    /// <summary>
    /// Eindeutiger Präfix für Help-Keys dieses Features (z. B. "Eckdaten").
    /// Lookup im HelpProvider erfolgt als "HelpKeyPrefix.PropertyName" bzw. HelpKeyPrefix für die Seite (Zero-State).
    /// </summary>
    protected abstract string HelpKeyPrefix { get; }

    /// <summary>
    /// HelpKey des aktuell fokussierten Feldes (wird von FocusHelpBehavior bei GotFocus gesetzt).
    /// </summary>
    public string? ActiveHelpKey
    {
        get => _activeHelpKey;
        set
        {
            if (SetProperty(ref _activeHelpKey, value))
            {
                RefreshHelpText();
                OnPropertyChanged(nameof(ActiveHelpErrors));
                OnPropertyChanged(nameof(HasActiveHelpErrors));
            }
        }
    }

    /// <summary>
    /// HelpKey für die Seite (Zero-State), wenn kein Feld fokussiert ist. In abgeleiteten ViewModels setzen.
    /// </summary>
    public string? PageHelpKey
    {
        get => _pageHelpKey;
        set
        {
            if (SetProperty(ref _pageHelpKey, value))
                RefreshHelpText();
        }
    }

    /// <summary>
    /// Titel für den aktuell angezeigten Hilfetext (aus <see cref="IHelpProvider"/>).
    /// </summary>
    public string? ActiveHelpTitle
    {
        get => _activeHelpTitle;
        private set => SetProperty(ref _activeHelpTitle, value);
    }

    /// <summary>
    /// Beschreibung für den aktuell angezeigten Hilfetext (aus <see cref="IHelpProvider"/>).
    /// </summary>
    public string? ActiveHelpDescription
    {
        get => _activeHelpDescription;
        private set => SetProperty(ref _activeHelpDescription, value);
    }

    /// <summary>
    /// Fehlermeldungen für das aktuell fokussierte Feld (für Bindung im InfoPanel).
    /// </summary>
    public IReadOnlyList<string> ActiveHelpErrors => GetErrors(ActiveHelpKey).Cast<string>().ToList();

    /// <summary>
    /// True, wenn für das aktuell fokussierte Feld Validierungsfehler vorliegen (für Sichtbarkeit des Fehler-Blocks).
    /// </summary>
    public bool HasActiveHelpErrors => ActiveHelpErrors.Count > 0;

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
        OnPropertyChanged(nameof(ActiveHelpErrors));
        OnPropertyChanged(nameof(HasActiveHelpErrors));
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
        OnPropertyChanged(nameof(ActiveHelpErrors));
        OnPropertyChanged(nameof(HasActiveHelpErrors));
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

    private void RefreshHelpText()
    {
        var key = !string.IsNullOrEmpty(ActiveHelpKey)
            ? $"{HelpKeyPrefix}.{ActiveHelpKey}"
            : (PageHelpKey ?? HelpKeyPrefix);
        if (_helpProvider != null && !string.IsNullOrEmpty(key) && _helpProvider.TryGetHelp(key, out var title, out var description))
        {
            ActiveHelpTitle = title;
            ActiveHelpDescription = description;
        }
        else
        {
            ActiveHelpTitle = null;
            ActiveHelpDescription = null;
        }
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
