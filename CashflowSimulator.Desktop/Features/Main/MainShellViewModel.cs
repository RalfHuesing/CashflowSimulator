using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Features.Settings;
using CashflowSimulator.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop.Features.Main;

/// <summary>
/// ViewModel für die Hauptshell.
/// Verwaltet die Anzeige und orchestriert Top-Level-Aktionen; der Projekt-State liegt in <see cref="ICurrentProjectService"/>.
/// </summary>
public partial class MainShellViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContentPlaceholderVisible))]
    private object? _currentContentViewModel;

    private readonly IFileDialogService _fileDialogService;
    private readonly IStorageService<SimulationProjectDto> _storageService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly Func<MetaEditViewModel> _createMetaEditViewModel;
    private readonly Func<SettingsViewModel> _createSettingsViewModel;
    private readonly ILogger<MainShellViewModel> _logger;

    public MainShellViewModel(
        IFileDialogService fileDialogService,
        IStorageService<SimulationProjectDto> storageService,
        ICurrentProjectService currentProjectService,
        Func<MetaEditViewModel> createMetaEditViewModel,
        Func<SettingsViewModel> createSettingsViewModel,
        NavigationViewModel navigationViewModel,
        ILogger<MainShellViewModel> logger)
    {
        _fileDialogService = fileDialogService;
        _storageService = storageService;
        _currentProjectService = currentProjectService;
        _createMetaEditViewModel = createMetaEditViewModel;
        _createSettingsViewModel = createSettingsViewModel;
        _logger = logger;
        Navigation = navigationViewModel;

        _currentProjectService.ProjectChanged += OnProjectChanged;
        InitializeNavigation();
        // Initialen Command-Status setzen (Projekt ist bereits vom Program gesetzt)
        SaveCommand.NotifyCanExecuteChanged();
        OpenSzenarioCommand.NotifyCanExecuteChanged();
    }

    public NavigationViewModel Navigation { get; }

    /// <summary>
    /// Dynamischer Titel für den Header-Bereich (aus <see cref="ICurrentProjectService"/>).
    /// </summary>
    public string CurrentProjectTitle => GetCurrentProjectTitle();

    /// <summary>
    /// Aktueller Dateipfad (aus <see cref="ICurrentProjectService"/>).
    /// </summary>
    public string? CurrentFilePath => _currentProjectService.CurrentFilePath;

    /// <summary>
    /// True, wenn der Platzhalter im Content-Bereich angezeigt werden soll (kein Bereich ausgewählt).
    /// </summary>
    public bool IsContentPlaceholderVisible => CurrentContentViewModel is null;

    private void OnProjectChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentProjectTitle));
        OnPropertyChanged(nameof(CurrentFilePath));
        SaveCommand.NotifyCanExecuteChanged();
        OpenSzenarioCommand.NotifyCanExecuteChanged();
    }

    private string GetCurrentProjectTitle()
    {
        var current = _currentProjectService.Current;
        if (current is null)
            return "Unbenanntes Szenario";
        return string.IsNullOrWhiteSpace(current.Meta.ScenarioName)
            ? "Unbenanntes Szenario"
            : current.Meta.ScenarioName;
    }

    private void InitializeNavigation()
    {
        var szenarioItem = new NavItemViewModel
        {
            DisplayName = "Szenario",
            Icon = Symbol.Database,
            Command = OpenSzenarioCommand,
            IsActive = false
        };
        var einstellungenItem = new NavItemViewModel
        {
            DisplayName = "Einstellungen",
            Icon = Symbol.Settings,
            Command = OpenSettingsCommand,
            IsActive = false
        };

        Navigation.Items.Add(szenarioItem);
        Navigation.Items.Add(einstellungenItem);
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
                return;
            }

            _currentProjectService.SetCurrent(result.Value!, path);
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
            var current = _currentProjectService.Current;
            if (current is null) return;

            var path = _currentProjectService.CurrentFilePath;

            if (string.IsNullOrEmpty(path))
            {
                path = await _fileDialogService.SaveAsync(new SaveFileDialogOptions(
                    "Szenario speichern",
                    "Szenario-Dateien",
                    "json",
                    SuggestedFileName: $"{current.Meta.ScenarioName}.json")).ConfigureAwait(true);

                if (string.IsNullOrEmpty(path)) return;
            }

            var result = await _storageService.SaveAsync(path, current).ConfigureAwait(true);
            if (!result.IsSuccess)
            {
                _logger.LogError("Fehler beim Speichern nach '{Path}': {Error}", path, result.Error);
                return;
            }

            _currentProjectService.SetCurrent(current, path);
            _logger.LogInformation("Projekt gespeichert unter {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unerwarteter Fehler beim SaveAsync.");
        }
    }

    private bool CanSave() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenSzenario))]
    private void OpenSzenario()
    {
        _logger.LogDebug("Szenario-Bereich geöffnet.");
        CurrentContentViewModel = _createMetaEditViewModel();
        SetActiveNavigationItem(0);
    }

    private bool CanOpenSzenario() => _currentProjectService.Current is not null;

    [RelayCommand]
    private void OpenSettings()
    {
        _logger.LogDebug("Einstellungen geöffnet.");
        CurrentContentViewModel = _createSettingsViewModel();
        SetActiveNavigationItem(1);
    }

    private void SetActiveNavigationItem(int index)
    {
        for (var i = 0; i < Navigation.Items.Count; i++)
            Navigation.Items[i].IsActive = i == index;
    }
}
