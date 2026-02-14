using System.Collections.Generic;
using System.Linq;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Settings;

/// <summary>
/// ViewModel für die Einstellungsseite (Theme-Auswahl, später weitere Optionen).
/// Schreibt in <see cref="ICurrentProjectService"/> und wendet Theme über <see cref="IThemeService"/> an.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly ICurrentProjectService _currentProjectService;

    public SettingsViewModel(IThemeService themeService, ICurrentProjectService currentProjectService)
    {
        _themeService = themeService;
        _currentProjectService = currentProjectService;

        var themes = _themeService.GetAvailableThemes();
        AvailableThemes = new List<ThemeOption>(themes);

        var currentId = _currentProjectService.Current?.UiSettings.SelectedThemeId;
        var id = string.IsNullOrWhiteSpace(currentId) ? _themeService.GetDefaultThemeId() : currentId.Trim();
        _selectedTheme = AvailableThemes.FirstOrDefault(t => t.Id == id) ?? AvailableThemes.First();
    }

    public IList<ThemeOption> AvailableThemes { get; }

    [ObservableProperty]
    private ThemeOption? _selectedTheme;

    partial void OnSelectedThemeChanged(ThemeOption? value)
    {
        if (value is null) return;
        _currentProjectService.UpdateUiSettings(new UiSettingsDto { SelectedThemeId = value.Id });
    }
}
