using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Meta;

/// <summary>
/// ViewModel für die Stammdaten-Seite (MetaDto: Szenario-Name, Erstellungsdatum).
/// Liest und schreibt über <see cref="ICurrentProjectService"/>; keine Callbacks.
/// </summary>
public partial class MetaEditViewModel : ObservableObject
{
    [ObservableProperty]
    private string _scenarioName = string.Empty;

    private readonly ICurrentProjectService _currentProjectService;

    public MetaEditViewModel(ICurrentProjectService currentProjectService)
    {
        _currentProjectService = currentProjectService;
        var meta = _currentProjectService.Current?.Meta;
        if (meta is not null)
        {
            ScenarioName = meta.ScenarioName;
            CreatedAt = meta.CreatedAt;
        }
    }

    public DateTimeOffset CreatedAt { get; private set; }

    [RelayCommand]
    private void Discard()
    {
        var meta = _currentProjectService.Current?.Meta;
        if (meta is not null)
        {
            ScenarioName = meta.ScenarioName;
            CreatedAt = meta.CreatedAt;
        }
    }

    [RelayCommand]
    private void Apply()
    {
        _currentProjectService.UpdateMeta(new MetaDto
        {
            ScenarioName = ScenarioName.Trim(),
            CreatedAt = CreatedAt
        });
    }
}
