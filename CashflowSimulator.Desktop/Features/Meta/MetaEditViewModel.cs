using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CashflowSimulator.Validation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Meta;

/// <summary>
/// ViewModel für die Stammdaten-Seite (MetaDto: Szenario-Name, Erstellungsdatum).
/// Liest und schreibt über <see cref="ICurrentProjectService"/>; Validierung über <see cref="ValidationRunner"/>.
/// Fehler inline am Control (INotifyDataErrorInfo); Auto-Save bei Änderung.
/// </summary>
public partial class MetaEditViewModel : ValidatingViewModelBase
{
    private const int ValidationDebounceMs = 300;

    [ObservableProperty]
    private string _scenarioName = string.Empty;

    private readonly ICurrentProjectService _currentProjectService;
    private bool _isLoading;

    public MetaEditViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "Szenario";
        LoadFromProject();
        ValidateAndSave();
    }

    public DateTimeOffset CreatedAt { get; private set; }

    partial void OnScenarioNameChanged(string value) => ScheduleValidateAndSave();

    private void ScheduleValidateAndSave()
    {
        if (_isLoading)
            return;
        ScheduleDebounced(ValidationDebounceMs, ValidateAndSave);
    }

    private void LoadFromProject()
    {
        var meta = _currentProjectService.Current?.Meta;
        if (meta is null)
            return;

        _isLoading = true;
        try
        {
            ScenarioName = meta.ScenarioName;
            CreatedAt = meta.CreatedAt;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ValidateAndSave()
    {
        var dto = new MetaDto
        {
            ScenarioName = ScenarioName.Trim(),
            CreatedAt = CreatedAt
        };

        var validationResult = ValidationRunner.Validate(dto);
        SetValidationErrors(validationResult.Errors);
        _currentProjectService.UpdateMeta(dto);
    }
}
