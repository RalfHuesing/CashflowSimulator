using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Features.Meta;
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
    [NotifyCanExecuteChangedFor(nameof(OpenSzenarioCommand))]
    [NotifyPropertyChangedFor(nameof(CurrentProjectTitle))]
    private SimulationProjectDto _currentProject;

    [ObservableProperty]
    private string? _currentFilePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContentPlaceholderVisible))]
    private object? _currentContentViewModel;

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

    /// <summary>
    /// True, wenn der Platzhalter im Content-Bereich angezeigt werden soll (kein Bereich ausgewählt).
    /// </summary>
    public bool IsContentPlaceholderVisible => CurrentContentViewModel is null;

    private void InitializeNavigation()
    {
        // Initialbefüllung der Navigation.
        // Später könnte dies basierend auf dem geladenen Projekt dynamisch erweitert werden.

        var szenarioItem = new NavItemViewModel
        {
            DisplayName = "Szenario",
            Icon = Symbol.Database,
            Command = OpenSzenarioCommand,
            IsActive = false
        };

        Navigation.Items.Add(szenarioItem);
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

    [RelayCommand(CanExecute = nameof(CanOpenSzenario))]
    private void OpenSzenario()
    {
        _logger.LogDebug("Szenario-Bereich geöffnet.");
        var onApply = new Action<MetaDto>(updated =>
        {
            CurrentProject = CurrentProject with { Meta = updated };
            _logger.LogDebug("Szenario-Metadaten übernommen: {Name}", updated.ScenarioName);
        });
        CurrentContentViewModel = new MetaEditViewModel(CurrentProject.Meta, onApply);
        if (Navigation.Items.Count > 0)
        {
            foreach (var item in Navigation.Items)
                item.IsActive = false;
            Navigation.Items[0].IsActive = true;
        }
    }

    private bool CanOpenSzenario() => CurrentProject is not null;
}