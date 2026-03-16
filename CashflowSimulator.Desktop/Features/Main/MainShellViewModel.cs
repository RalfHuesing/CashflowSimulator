using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.CashflowEvents;
using CashflowSimulator.Desktop.Features.CashflowStreams;
using CashflowSimulator.Desktop.Features.Eckdaten;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Features.Marktdaten;
using CashflowSimulator.Desktop.Features.Korrelationen;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Features.Portfolio;
using CashflowSimulator.Desktop.Features.Settings;
using CashflowSimulator.Desktop.Features.SimulationResult;
using CashflowSimulator.Desktop.Features.Analysis;
using CashflowSimulator.Desktop.Features.TaxProfiles;
using CashflowSimulator.Desktop.Features.StrategyProfiles;
using CashflowSimulator.Desktop.Features.AllocationProfiles;
using CashflowSimulator.Desktop.Features.LifecyclePhases;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Desktop.ViewModels;
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
    private object? _currentContentViewModel;

    private readonly IFileDialogService _fileDialogService;
    private readonly IStorageService<SimulationProjectDto> _storageService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly INavigationConfiguration _navigationConfiguration;
    private readonly INavigationService _navigationService;
    private readonly ISimulationRunner _simulationRunner;
    private readonly IResultAnalysisService _resultAnalysisService;
    private readonly IDiagnosticExportService _diagnosticExportService;
    private readonly ILogger<MainShellViewModel> _logger;

    public MainShellViewModel(
        IFileDialogService fileDialogService,
        IStorageService<SimulationProjectDto> storageService,
        ICurrentProjectService currentProjectService,
        INavigationService navigationService,
        ISimulationRunner simulationRunner,
        IResultAnalysisService resultAnalysisService,
        IDiagnosticExportService diagnosticExportService,
        NavigationViewModel navigationViewModel,
        INavigationConfiguration navigationConfiguration,
        ILogger<MainShellViewModel> logger)
    {
        _fileDialogService = fileDialogService;
        _storageService = storageService;
        _currentProjectService = currentProjectService;
        _navigationService = navigationService;
        _simulationRunner = simulationRunner;
        _resultAnalysisService = resultAnalysisService;
        _diagnosticExportService = diagnosticExportService;
        _logger = logger;
        _navigationConfiguration = navigationConfiguration;
        Navigation = navigationViewModel;

        _currentProjectService.ProjectChanged += OnProjectChanged;
        
        _navigationConfiguration.Configure(Navigation, NavigateAsync, () => _currentProjectService.Current is not null);
        RefreshNavigationCommands();
    }
    
    private void RefreshNavigationCommands()
    {
        foreach (var item in Navigation.GetAllItems())
        {
            if (item.Command is IRelayCommand rc)
                rc.NotifyCanExecuteChanged();
        }
        SaveCommand.NotifyCanExecuteChanged();
        StartSimulationCommand.NotifyCanExecuteChanged();
    }

    public NavigationViewModel Navigation { get; }

    /// <summary>
    /// Aktuelles Feature-ViewModel im Content-Bereich (null = Platzhalter).
    /// </summary>
    public object? CurrentContentViewModel
    {
        get => _currentContentViewModel;
        set
        {
            var previous = _currentContentViewModel;
            if (SetProperty(ref _currentContentViewModel, value))
            {
                if (previous is ValidatingViewModelBase validatingVm)
                    validatingVm.ClearStatus();
                OnPropertyChanged(nameof(IsContentPlaceholderVisible));
                if (value is IDiagnosticExport export && !string.IsNullOrWhiteSpace(_currentProjectService.LastRunFolderPath))
                    _ = _diagnosticExportService.ExportAsync(export);
            }
        }
    }

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
        RefreshNavigationCommands();
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

            var project = result.Value!;
            _currentProjectService.SetCurrent(project, path);

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

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task StartSimulationAsync()
    {
        var project = _currentProjectService.Current;
        if (project is null) return;

        try
        {
            var result = await _simulationRunner.RunSimulationAsync(project).ConfigureAwait(true);
            var runId = result.RunId ?? 0L;
            _currentProjectService.SetLastRunId(runId, result.ResultFolderPath);
            var resultViewModel = _navigationService.Create<SimulationResultViewModel>(runId);
            await resultViewModel.LoadAsync().ConfigureAwait(true);
            CurrentContentViewModel = resultViewModel;
            var count = resultViewModel.MonthlyResults.Count;
            _logger.LogInformation("Simulation abgeschlossen: {Count} Monate", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Simulation fehlgeschlagen.");
        }
    }

    private async Task NavigateAsync(Type targetType, object[]? parameters)
    {
        try
        {
            _logger.LogDebug("Navigation zu {Type} gestartet.", targetType.Name);
            if (targetType == typeof(AnalysisDashboardViewModel))
            {
                var runId = _currentProjectService.LastRunId ?? 0L;
                var analysisVm = (AnalysisDashboardViewModel)_navigationService.Create(typeof(AnalysisDashboardViewModel), runId);
                await analysisVm.LoadAsync().ConfigureAwait(true);
                CurrentContentViewModel = analysisVm;
            }
            else
            {
                CurrentContentViewModel = parameters is { Length: > 0 } 
                    ? _navigationService.Create(targetType, parameters) 
                    : _navigationService.Create(targetType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Navigation zu {Type}", targetType.Name);
        }
    }

}
