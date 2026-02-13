using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop.Features.Main;

/// <summary>
/// ViewModel für die Hauptshell.
/// Verwaltet den globalen Status (Projekt, Dateipfad) und orchestriert die Top-Level-Aktionen.
/// </summary>
public partial class MainShellViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenStammdatenCommand))]
    [NotifyPropertyChangedFor(nameof(CurrentProjectTitle))]
    private SimulationProjectDto _currentProject;

    [ObservableProperty]
    private string? _currentFilePath;

    private readonly IFileDialogService _fileDialogService;
    private readonly IStorageService<SimulationProjectDto> _storageService;
    private readonly IDefaultProjectProvider _defaultProjectProvider;
    private readonly ILogger<MainShellViewModel> _logger;

    public MainShellViewModel(
        IFileDialogService fileDialogService,
        IStorageService<SimulationProjectDto> storageService,
        IDefaultProjectProvider defaultProjectProvider,
        NavigationViewModel navigationViewModel,
        ILogger<MainShellViewModel> logger)
    {
        _fileDialogService = fileDialogService;
        _storageService = storageService;
        _defaultProjectProvider = defaultProjectProvider;
        _logger = logger;
        Navigation = navigationViewModel;

        // Startzustand: Default-Projekt (InMemory)
        CurrentProject = _defaultProjectProvider.CreateDefault();
        CurrentFilePath = null;

        InitializeNavigation();
    }

    public NavigationViewModel Navigation { get; }

    /// <summary>
    /// Dynamischer Titel für den Header-Bereich.
    /// </summary>
    public string CurrentProjectTitle => string.IsNullOrWhiteSpace(CurrentProject.Meta.ScenarioName)
        ? "Unbenanntes Szenario"
        : CurrentProject.Meta.ScenarioName;

    private void InitializeNavigation()
    {
        // Initialbefüllung der Navigation.
        // Später könnte dies basierend auf dem geladenen Projekt dynamisch erweitert werden.

        var stammdatenItem = new NavItemViewModel
        {
            DisplayName = "Stammdaten",
            Icon = Symbol.Database, // FluentIcon: Database passt gut zu Meta-Daten
            Command = OpenStammdatenCommand,
            IsActive = true
        };

        Navigation.Items.Add(stammdatenItem);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            var path = await _fileDialogService.OpenAsync(
                new FileDialogOptions("Szenario öffnen", "Szenario-Dateien", ".json")).ConfigureAwait(true);

            if (string.IsNullOrEmpty(path)) return;

            var result = await _storageService.LoadAsync(path).ConfigureAwait(true);
            if (!result.IsSuccess)
            {
                _logger.LogError("Fehler beim Laden der Datei '{Path}': {Error}", path, result.Error);
                // Hier könnte man noch einen User-Dialog (MessageBox) triggern
                return;
            }

            CurrentProject = result.Value!;
            CurrentFilePath = path;
            _logger.LogInformation("Projekt erfolgreich geladen aus {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unerwarteter Fehler beim LoadAsync.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            var path = CurrentFilePath;

            // "Speichern unter" Logik, falls noch kein Pfad existiert
            if (string.IsNullOrEmpty(path))
            {
                path = await _fileDialogService.SaveAsync(new SaveFileDialogOptions(
                    "Szenario speichern",
                    "Szenario-Dateien",
                    "json",
                    SuggestedFileName: $"{CurrentProject.Meta.ScenarioName}.json")).ConfigureAwait(true);

                if (string.IsNullOrEmpty(path)) return;
            }

            var result = await _storageService.SaveAsync(path, CurrentProject).ConfigureAwait(true);
            if (!result.IsSuccess)
            {
                _logger.LogError("Fehler beim Speichern nach '{Path}': {Error}", path, result.Error);
                return;
            }

            CurrentFilePath = path;
            _logger.LogInformation("Projekt gespeichert unter {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unerwarteter Fehler beim SaveAsync.");
        }
    }

    private bool CanSave() => CurrentProject is not null;

    [RelayCommand(CanExecute = nameof(CanOpenStammdaten))]
    private async Task OpenStammdatenAsync()
    {
        try
        {
#if false
            var updated = await _metaEditDialogService.ShowEditAsync(CurrentProject.Meta).ConfigureAwait(true);
            if (updated is null) return; // Abbrechen

            // Immutable Update: Wir erstellen eine Kopie des Projects mit neuen Meta-Daten
            CurrentProject = CurrentProject with { Meta = updated };
            _logger.LogDebug("Stammdaten aktualisiert: {Name}", updated.ScenarioName);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler im Stammdaten-Dialog.");
        }
    }

    private bool CanOpenStammdaten() => CurrentProject is not null;
}