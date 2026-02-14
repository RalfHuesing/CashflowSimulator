using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Settings;

/// <summary>
/// ViewModel für die Einstellungsseite (Theme-Auswahl, später weitere Optionen).
/// Schreibt in <see cref="ICurrentProjectService"/>; Theme-Wechsel mit Debounce (300 ms), damit kein Wechsel während offenem ComboBox-Dropdown.
/// Erbt von <see cref="ValidatingViewModelBase"/> für FeatureLayout/Statusleiste; keine Validierung.
/// </summary>
public partial class SettingsViewModel : ValidatingViewModelBase
{
    private readonly ICurrentProjectService _currentProjectService;

    private CancellationTokenSource? _debounceCts;

    public SettingsViewModel(IThemeService themeService, ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        _currentProjectService = currentProjectService;
        PageHelpKey = "Einstellungen";

        var themes = themeService.GetAvailableThemes();
        AvailableThemes = new List<ThemeOption>(themes);

        var currentId = _currentProjectService.Current?.UiSettings.SelectedThemeId;
        var id = string.IsNullOrWhiteSpace(currentId) ? themeService.GetDefaultThemeId() : currentId.Trim();
        _selectedTheme = AvailableThemes.FirstOrDefault(t => t.Id == id) ?? AvailableThemes.First();
    }

    public IList<ThemeOption> AvailableThemes { get; }

    [ObservableProperty]
    private ThemeOption? _selectedTheme;

    partial void OnSelectedThemeChanged(ThemeOption? value)
    {
        if (value is null) return;

        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = ChangeThemeDebouncedAsync(value, token);
    }

    private async Task ChangeThemeDebouncedAsync(ThemeOption theme, CancellationToken token)
    {
        try
        {
            await Task.Delay(300, token).ConfigureAwait(true);

            if (token.IsCancellationRequested) return;

            _currentProjectService.UpdateUiSettings(new UiSettingsDto { SelectedThemeId = theme.Id });
        }
        catch (TaskCanceledException)
        {
            // Gewollt bei erneutem Wechsel innerhalb der 300 ms
        }
    }
}
