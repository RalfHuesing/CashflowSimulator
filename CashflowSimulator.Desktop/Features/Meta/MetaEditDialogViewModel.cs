using CashflowSimulator.Contracts.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Meta;

/// <summary>
/// ViewModel für den Stammdaten-Dialog (MetaDto: Szenario-Name, Erstellungsdatum).
/// </summary>
public partial class MetaEditDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _scenarioName = string.Empty;

    public MetaEditDialogViewModel(MetaDto current)
    {
        ScenarioName = current.ScenarioName;
        CreatedAt = current.CreatedAt;
    }

    /// <summary>
    /// Wird vom Dialog gesetzt; Aufruf schließt das Fenster mit dem übergebenen Ergebnis (oder null bei Abbrechen).
    /// </summary>
    public Action<MetaDto?>? CloseWithResult { get; set; }

    public DateTimeOffset CreatedAt { get; }

    [RelayCommand]
    private void Ok()
    {
        CloseWithResult?.Invoke(new MetaDto
        {
            ScenarioName = ScenarioName.Trim(),
            CreatedAt = CreatedAt
        });
    }

    [RelayCommand]
    private void Cancel() => CloseWithResult?.Invoke(null);
}
