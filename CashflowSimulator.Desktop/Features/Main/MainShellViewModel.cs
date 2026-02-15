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
using CashflowSimulator.Desktop.Features.TaxProfiles;
using CashflowSimulator.Desktop.Features.StrategyProfiles;
using CashflowSimulator.Desktop.Features.LifecyclePhases;
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
    private object? _currentContentViewModel;

    private readonly IFileDialogService _fileDialogService;
    private readonly IStorageService<SimulationProjectDto> _storageService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<MainShellViewModel> _logger;

    public MainShellViewModel(
        IFileDialogService fileDialogService,
        IStorageService<SimulationProjectDto> storageService,
        ICurrentProjectService currentProjectService,
        INavigationService navigationService,
        NavigationViewModel navigationViewModel,
        ILogger<MainShellViewModel> logger)
    {
        _fileDialogService = fileDialogService;
        _storageService = storageService;
        _currentProjectService = currentProjectService;
        _navigationService = navigationService;
        _logger = logger;
        Navigation = navigationViewModel;

        _currentProjectService.ProjectChanged += OnProjectChanged;
        InitializeNavigation();
        // Initialen Command-Status setzen (Projekt ist bereits vom Program gesetzt)
        SaveCommand.NotifyCanExecuteChanged();
        OpenSzenarioCommand.NotifyCanExecuteChanged();
        OpenEckdatenCommand.NotifyCanExecuteChanged();
        OpenLaufendeEinnahmenCommand.NotifyCanExecuteChanged();
        OpenLaufendeAusgabenCommand.NotifyCanExecuteChanged();
        OpenGeplanteEinnahmenCommand.NotifyCanExecuteChanged();
        OpenGeplanteAusgabenCommand.NotifyCanExecuteChanged();
        OpenMarktdatenCommand.NotifyCanExecuteChanged();
        OpenKorrelationenCommand.NotifyCanExecuteChanged();
        OpenAnlageklassenCommand.NotifyCanExecuteChanged();
        OpenVermoegenswerteCommand.NotifyCanExecuteChanged();
        OpenTransaktionenCommand.NotifyCanExecuteChanged();
        OpenSteuerprofileCommand.NotifyCanExecuteChanged();
        OpenStrategieprofileCommand.NotifyCanExecuteChanged();
        OpenLebensphasenCommand.NotifyCanExecuteChanged();
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
            if (SetProperty(ref _currentContentViewModel, value))
                OnPropertyChanged(nameof(IsContentPlaceholderVisible));
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
        SaveCommand.NotifyCanExecuteChanged();
        OpenSzenarioCommand.NotifyCanExecuteChanged();
        OpenEckdatenCommand.NotifyCanExecuteChanged();
        OpenLaufendeEinnahmenCommand.NotifyCanExecuteChanged();
        OpenLaufendeAusgabenCommand.NotifyCanExecuteChanged();
        OpenGeplanteEinnahmenCommand.NotifyCanExecuteChanged();
        OpenGeplanteAusgabenCommand.NotifyCanExecuteChanged();
        OpenMarktdatenCommand.NotifyCanExecuteChanged();
        OpenKorrelationenCommand.NotifyCanExecuteChanged();
        OpenAnlageklassenCommand.NotifyCanExecuteChanged();
        OpenVermoegenswerteCommand.NotifyCanExecuteChanged();
        OpenTransaktionenCommand.NotifyCanExecuteChanged();
        OpenSteuerprofileCommand.NotifyCanExecuteChanged();
        OpenStrategieprofileCommand.NotifyCanExecuteChanged();
        OpenLebensphasenCommand.NotifyCanExecuteChanged();
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
        var eckdatenItem = new NavItemViewModel
        {
            DisplayName = "Eckdaten",
            Icon = Symbol.Calendar,
            Command = OpenEckdatenCommand,
            IsActive = false
        };
        var marktdatenItem = new NavItemViewModel
        {
            DisplayName = "Marktdaten",
            Icon = Symbol.ChartMultiple,
            Command = OpenMarktdatenCommand,
            IsActive = false
        };
        var korrelationenItem = new NavItemViewModel
        {
            DisplayName = "Korrelationen",
            Icon = Symbol.Link,
            Command = OpenKorrelationenCommand,
            IsActive = false
        };
        var anlageklassenItem = new NavItemViewModel
        {
            DisplayName = "Anlageklassen",
            Icon = Symbol.Grid,
            Command = OpenAnlageklassenCommand,
            IsActive = false
        };
        var vermoegenswerteItem = new NavItemViewModel
        {
            DisplayName = "Vermögenswerte",
            Icon = Symbol.Stack,
            Command = OpenVermoegenswerteCommand,
            IsActive = false
        };
        var transaktionenItem = new NavItemViewModel
        {
            DisplayName = "Transaktionen",
            Icon = Symbol.Document,
            Command = OpenTransaktionenCommand,
            IsActive = false
        };
        var laufendeEinnahmenItem = new NavItemViewModel
        {
            DisplayName = "Laufende Einnahmen",
            Icon = Symbol.ArrowUp,
            Command = OpenLaufendeEinnahmenCommand,
            IsActive = false
        };
        var laufendeAusgabenItem = new NavItemViewModel
        {
            DisplayName = "Laufende Ausgaben",
            Icon = Symbol.ArrowDown,
            Command = OpenLaufendeAusgabenCommand,
            IsActive = false
        };
        var geplanteEinnahmenItem = new NavItemViewModel
        {
            DisplayName = "Geplante Einnahmen",
            Icon = Symbol.CalendarAdd,
            Command = OpenGeplanteEinnahmenCommand,
            IsActive = false
        };
        var geplanteAusgabenItem = new NavItemViewModel
        {
            DisplayName = "Geplante Ausgaben",
            Icon = Symbol.CalendarCancel,
            Command = OpenGeplanteAusgabenCommand,
            IsActive = false
        };
        var steuerprofileItem = new NavItemViewModel
        {
            DisplayName = "Steuerprofile",
            Icon = Symbol.Receipt,
            Command = OpenSteuerprofileCommand,
            IsActive = false
        };
        var strategieprofileItem = new NavItemViewModel
        {
            DisplayName = "Strategieprofile",
            Icon = Symbol.Target,
            Command = OpenStrategieprofileCommand,
            IsActive = false
        };
        var lebensphasenItem = new NavItemViewModel
        {
            DisplayName = "Lebensphasen",
            Icon = Symbol.Person,
            Command = OpenLebensphasenCommand,
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
        Navigation.Items.Add(eckdatenItem);
        Navigation.Items.Add(marktdatenItem);
        Navigation.Items.Add(korrelationenItem);
        Navigation.Items.Add(anlageklassenItem);
        Navigation.Items.Add(vermoegenswerteItem);
        Navigation.Items.Add(transaktionenItem);
        Navigation.Items.Add(laufendeEinnahmenItem);
        Navigation.Items.Add(laufendeAusgabenItem);
        Navigation.Items.Add(geplanteEinnahmenItem);
        Navigation.Items.Add(geplanteAusgabenItem);
        Navigation.Items.Add(steuerprofileItem);
        Navigation.Items.Add(strategieprofileItem);
        Navigation.Items.Add(lebensphasenItem);
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

    [RelayCommand(CanExecute = nameof(CanOpenSzenario))]
    private void OpenSzenario()
    {
        _logger.LogDebug("Szenario-Bereich geöffnet.");
        CurrentContentViewModel = _navigationService.Create<MetaEditViewModel>();
        SetActiveNavigationItem(0);
    }

    private bool CanOpenSzenario() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenEckdaten))]
    private void OpenEckdaten()
    {
        _logger.LogDebug("Eckdaten geöffnet.");
        CurrentContentViewModel = _navigationService.Create<EckdatenViewModel>();
        SetActiveNavigationItem(1);
    }

    private bool CanOpenEckdaten() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenMarktdaten))]
    private void OpenMarktdaten()
    {
        _logger.LogDebug("Marktdaten geöffnet.");
        CurrentContentViewModel = _navigationService.Create<MarktdatenViewModel>();
        SetActiveNavigationItem(2);
    }

    private bool CanOpenMarktdaten() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenKorrelationen))]
    private void OpenKorrelationen()
    {
        _logger.LogDebug("Korrelationen geöffnet.");
        CurrentContentViewModel = _navigationService.Create<KorrelationenViewModel>();
        SetActiveNavigationItem(3);
    }

    private bool CanOpenKorrelationen() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenAnlageklassen))]
    private void OpenAnlageklassen()
    {
        _logger.LogDebug("Anlageklassen geöffnet.");
        CurrentContentViewModel = _navigationService.Create<AssetClassesViewModel>();
        SetActiveNavigationItem(4);
    }

    private bool CanOpenAnlageklassen() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenVermoegenswerte))]
    private void OpenVermoegenswerte()
    {
        _logger.LogDebug("Vermögenswerte geöffnet.");
        CurrentContentViewModel = _navigationService.Create<PortfolioViewModel>();
        SetActiveNavigationItem(5);
    }

    private bool CanOpenVermoegenswerte() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenTransaktionen))]
    private void OpenTransaktionen()
    {
        _logger.LogDebug("Transaktionen geöffnet.");
        CurrentContentViewModel = _navigationService.Create<TransactionsViewModel>();
        SetActiveNavigationItem(6);
    }

    private bool CanOpenTransaktionen() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenCashflow))]
    private void OpenLaufendeEinnahmen()
    {
        CurrentContentViewModel = _navigationService.Create<CashflowStreamsViewModel>(CashflowType.Income);
        SetActiveNavigationItem(7);
    }

    [RelayCommand(CanExecute = nameof(CanOpenCashflow))]
    private void OpenLaufendeAusgaben()
    {
        CurrentContentViewModel = _navigationService.Create<CashflowStreamsViewModel>(CashflowType.Expense);
        SetActiveNavigationItem(8);
    }

    [RelayCommand(CanExecute = nameof(CanOpenCashflow))]
    private void OpenGeplanteEinnahmen()
    {
        CurrentContentViewModel = _navigationService.Create<CashflowEventsViewModel>(CashflowType.Income);
        SetActiveNavigationItem(9);
    }

    [RelayCommand(CanExecute = nameof(CanOpenCashflow))]
    private void OpenGeplanteAusgaben()
    {
        CurrentContentViewModel = _navigationService.Create<CashflowEventsViewModel>(CashflowType.Expense);
        SetActiveNavigationItem(10);
    }

    private bool CanOpenCashflow() => _currentProjectService.Current is not null;

    [RelayCommand(CanExecute = nameof(CanOpenLifecycle))]
    private void OpenSteuerprofile()
    {
        _logger.LogDebug("Steuerprofile geöffnet.");
        CurrentContentViewModel = _navigationService.Create<TaxProfilesViewModel>();
        SetActiveNavigationItem(11);
    }

    [RelayCommand(CanExecute = nameof(CanOpenLifecycle))]
    private void OpenStrategieprofile()
    {
        _logger.LogDebug("Strategieprofile geöffnet.");
        CurrentContentViewModel = _navigationService.Create<StrategyProfilesViewModel>();
        SetActiveNavigationItem(12);
    }

    [RelayCommand(CanExecute = nameof(CanOpenLifecycle))]
    private void OpenLebensphasen()
    {
        _logger.LogDebug("Lebensphasen geöffnet.");
        CurrentContentViewModel = _navigationService.Create<LifecyclePhasesViewModel>();
        SetActiveNavigationItem(13);
    }

    private bool CanOpenLifecycle() => _currentProjectService.Current is not null;

    [RelayCommand]
    private void OpenSettings()
    {
        _logger.LogDebug("Einstellungen geöffnet.");
        CurrentContentViewModel = _navigationService.Create<SettingsViewModel>();
        SetActiveNavigationItem(14);
    }

    private void SetActiveNavigationItem(int index)
    {
        for (var i = 0; i < Navigation.Items.Count; i++)
            Navigation.Items[i].IsActive = i == index;
    }

}
