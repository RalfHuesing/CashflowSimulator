using CashflowSimulator.Contracts.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Meta;

/// <summary>
/// ViewModel für die Stammdaten-Seite (MetaDto: Szenario-Name, Erstellungsdatum).
/// Inline im Hauptbereich, keine Dialog-Logik.
/// </summary>
public partial class MetaEditViewModel : ObservableObject
{
    [ObservableProperty]
    private string _scenarioName = string.Empty;

    private readonly Action<MetaDto> _onApply;

    public MetaEditViewModel(MetaDto current, Action<MetaDto> onApply)
    {
        ScenarioName = current.ScenarioName;
        CreatedAt = current.CreatedAt;
        _onApply = onApply;
    }

    public DateTimeOffset CreatedAt { get; }

    [RelayCommand]
    private void Apply()
    {
        _onApply(new MetaDto
        {
            ScenarioName = ScenarioName.Trim(),
            CreatedAt = CreatedAt
        });
    }
}
