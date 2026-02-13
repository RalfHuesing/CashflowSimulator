using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CashflowSimulator.Desktop.Features.Main;

/// <summary>
/// ViewModel für die Hauptshell: Banner, Navigation, Content, Laden/Speichern.
/// Hält das aktuelle Projekt und den aktuellen Dateipfad; orchestriert Dateidialog und Storage.
/// </summary>
public partial class MainShellViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowStammdatenCommand))]
    [NotifyPropertyChangedFor(nameof(CurrentProjectTitle))]
    private SimulationProjectDto _currentProject;

    [ObservableProperty]
    private string? _currentFilePath;

    [ObservableProperty]
    private object? _currentContent;

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

        CurrentProject = _defaultProjectProvider.CreateDefault();
        CurrentFilePath = null;

        var stammdatenItem = new NavItemViewModel { DisplayName = "Stammdaten", Command = ShowStammdatenCommand };
        navigationViewModel.Items.Add(stammdatenItem);

        ShowStammdaten();
    }

    /// <summary>
    /// Anzeige für Titel/Banner: Projektname oder "Neues Szenario".
    /// </summary>
    public string CurrentProjectTitle => string.IsNullOrEmpty(CurrentProject.Meta.ScenarioName)
        ? "Neues Szenario"
        : CurrentProject.Meta.ScenarioName;

    public NavigationViewModel Navigation { get; }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var path = await _fileDialogService.OpenAsync(
            new FileDialogOptions("Szenario öffnen", "Szenario-Dateien", ".json")).ConfigureAwait(true);
        if (string.IsNullOrEmpty(path)) return;

        var result = await _storageService.LoadAsync(path).ConfigureAwait(true);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Laden fehlgeschlagen: {Error}", result.Error);
            return;
        }

        CurrentProject = result.Value!;
        CurrentFilePath = path;
        _logger.LogInformation("Projekt geladen: {Path}", path);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
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

    private bool CanSave() => CurrentProject is not null;

    [RelayCommand(CanExecute = nameof(CanShowStammdaten))]
    private void ShowStammdaten()
    {
        CurrentContent = new MetaEditViewModel(CurrentProject.Meta, meta =>
        {
            CurrentProject = CurrentProject with { Meta = meta };
        });
        foreach (var item in Navigation.Items)
            item.IsActive = item.DisplayName == "Stammdaten";
    }

    private bool CanShowStammdaten() => CurrentProject is not null;
}
