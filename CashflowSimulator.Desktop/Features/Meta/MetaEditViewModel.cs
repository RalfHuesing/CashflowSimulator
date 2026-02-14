using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CashflowSimulator.Desktop.Features.Meta;

/// <summary>
/// ViewModel für die Stammdaten-Seite (MetaDto: Szenario-Name, Erstellungsdatum).
/// Liest und schreibt über <see cref="ICurrentProjectService"/>; Validierung über <see cref="ValidationRunner"/>.
/// </summary>
public partial class MetaEditViewModel : ObservableObject
{
    [ObservableProperty]
    private string _scenarioName = string.Empty;

    private readonly ICurrentProjectService _currentProjectService;
    private readonly IValidationMessageService _validationMessageService;

    public MetaEditViewModel(ICurrentProjectService currentProjectService, IValidationMessageService validationMessageService)
    {
        _currentProjectService = currentProjectService;
        _validationMessageService = validationMessageService;
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
        var dto = new MetaDto
        {
            ScenarioName = ScenarioName.Trim(),
            CreatedAt = CreatedAt
        };

        var validationResult = ValidationRunner.Validate(dto);
        if (!validationResult.IsValid)
        {
            _validationMessageService.SetErrors("Szenario", validationResult.Errors);
            return;
        }

        _validationMessageService.ClearSource("Szenario");
        _currentProjectService.UpdateMeta(dto);
    }
}
