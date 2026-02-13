using System.ComponentModel;
using System.Windows.Input;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Common;

namespace CashflowSimulator.Desktop.Features.Meta;

/// <summary>
/// ViewModel für den Stammdaten-Dialog (MetaDto: Szenario-Name, Erstellungsdatum).
/// </summary>
public class MetaEditDialogViewModel : INotifyPropertyChanged
{
    private string _scenarioName = string.Empty;

    public MetaEditDialogViewModel(MetaDto current)
    {
        _scenarioName = current.ScenarioName;
        CreatedAt = current.CreatedAt;

        OkCommand = new RelayCommand(OnOk, () => true);
        CancelCommand = new RelayCommand(OnCancel, () => true);
    }

    /// <summary>
    /// Wird vom Dialog gesetzt; Aufruf schließt das Fenster mit dem übergebenen Ergebnis (oder null bei Abbrechen).
    /// </summary>
    public Action<MetaDto?>? CloseWithResult { get; set; }

    public string ScenarioName
    {
        get => _scenarioName;
        set
        {
            if (_scenarioName == value) return;
            _scenarioName = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public DateTimeOffset CreatedAt { get; }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    private void OnOk()
    {
        CloseWithResult?.Invoke(new MetaDto
        {
            ScenarioName = ScenarioName.Trim(),
            CreatedAt = CreatedAt
        });
    }

    private void OnCancel() => CloseWithResult?.Invoke(null);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
