using System.ComponentModel;
using System.Windows.Input;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Common;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Services;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop.Features.Main;

/// <summary>
/// ViewModel für die Hauptshell: Banner, Navigation, Content, Laden/Speichern.
/// Hält das aktuelle Projekt und den aktuellen Dateipfad; orchestriert Dateidialog und Storage.
/// </summary>
public class MainShellViewModel : INotifyPropertyChanged
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IStorageService<SimulationProjectDto> _storageService;
    private readonly IMetaEditDialogService _metaEditDialogService;
    private readonly ILogger<MainShellViewModel> _logger;

    public MainShellViewModel(
        IFileDialogService fileDialogService,
        IStorageService<SimulationProjectDto> storageService,
        IMetaEditDialogService metaEditDialogService,
        NavigationViewModel navigationViewModel,
        ILogger<MainShellViewModel> logger)
    {
        _fileDialogService = fileDialogService;
        _storageService = storageService;
        _metaEditDialogService = metaEditDialogService;
        _logger = logger;
        Navigation = navigationViewModel;

        LoadCommand = new RelayCommand(LoadAsync, () => true);
        SaveCommand = new RelayCommand(SaveAsync, () => CurrentProject is not null);
        OpenStammdatenCommand = new RelayCommand(OpenStammdatenAsync, () => CurrentProject is not null);

        // Leeres Projekt als Startzustand
        _currentProject = NewEmptyProject();
        _currentFilePath = null;

        // Navigationseinträge (IconKeys optional, später z. B. FluentIcons)
        navigationViewModel.Items.Add(new NavItemViewModel { DisplayName = "Stammdaten", Command = OpenStammdatenCommand });
    }

    /// <summary>
    /// Aktuell geladenes Projekt (niemals null; bei Start ein leeres Projekt).
    /// </summary>
    public SimulationProjectDto CurrentProject
    {
        get => _currentProject;
        private set
        {
            if (_currentProject == value) return;
            _currentProject = value;
            OnPropertyChanged();
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            ((RelayCommand)OpenStammdatenCommand).RaiseCanExecuteChanged();
        }
    }

    private SimulationProjectDto _currentProject;

    /// <summary>
    /// Pfad der aktuell geladenen Datei; null wenn noch nicht gespeichert / neu.
    /// </summary>
    public string? CurrentFilePath
    {
        get => _currentFilePath;
        private set
        {
            if (_currentFilePath == value) return;
            _currentFilePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentProjectTitle));
        }
    }

    private string? _currentFilePath;

    /// <summary>
    /// Anzeige für Titel/Banner: Projektname oder "Neues Szenario".
    /// </summary>
    public string CurrentProjectTitle => string.IsNullOrEmpty(CurrentProject.Meta.ScenarioName)
        ? "Neues Szenario"
        : CurrentProject.Meta.ScenarioName;

    public NavigationViewModel Navigation { get; }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand OpenStammdatenCommand { get; }

    private async void LoadAsync()
    {
        var path = await _fileDialogService.OpenAsync(
            new FileDialogOptions("Szenario öffnen", "Szenario-Dateien", ".json")).ConfigureAwait(true);
        if (string.IsNullOrEmpty(path)) return;

        var result = await _storageService.LoadAsync(path).ConfigureAwait(true);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Laden fehlgeschlagen: {Error}", result.Error);
            // TODO: Fehler an Nutzer (Toast/Message)
            return;
        }

        CurrentProject = result.Value!;
        CurrentFilePath = path;
        _logger.LogInformation("Projekt geladen: {Path}", path);
    }

    private async void SaveAsync()
    {
        var path = CurrentFilePath;
        if (string.IsNullOrEmpty(path))
        {
            path = await _fileDialogService.SaveAsync(new SaveFileDialogOptions(
                "Szenario speichern",
                "Szenario-Dateien",
                "json",
                SuggestedFileName: "Szenario.json")).ConfigureAwait(true);
            if (string.IsNullOrEmpty(path)) return;
        }

        var result = await _storageService.SaveAsync(path, CurrentProject).ConfigureAwait(true);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Speichern fehlgeschlagen: {Error}", result.Error);
            return;
        }

        CurrentFilePath = path;
        _logger.LogInformation("Projekt gespeichert: {Path}", path);
    }

    private async void OpenStammdatenAsync()
    {
        var updated = await _metaEditDialogService.ShowEditAsync(CurrentProject.Meta).ConfigureAwait(true);
        if (updated is null) return;
        CurrentProject = CurrentProject with { Meta = updated };
    }

    private static SimulationProjectDto NewEmptyProject() => new()
    {
        Meta = new MetaDto { ScenarioName = "", CreatedAt = DateTimeOffset.UtcNow },
        Parameters = new SimulationParametersDto()
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
