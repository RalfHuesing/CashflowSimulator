using System.Reflection;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using Avalonia.Markup.Xaml.Styling;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Wendet zur Laufzeit Fluent- oder Simple-Basis-Theme an, optional mit Custom-Overlay.
/// Wird beim Start und bei Projektwechsel (SetCurrent / UpdateUiSettings) genutzt.
/// </summary>
public sealed class ThemeService : IThemeService
{
    public const string IdFluent = "Fluent";
    public const string IdSimple = "Simple";

    /// <summary>
    /// Beide Optionen laden immer unser Default.axaml-Overlay (Ressourcen wie TextPrimaryBrush, form-label etc.).
    /// Es wird nur das Basis-Theme (Fluent vs. Simple) getauscht.
    /// </summary>
    private static readonly IReadOnlyList<ThemeOption> Themes = new[]
    {
        new ThemeOption(IdFluent, "Fluent (Standard)"),
        new ThemeOption(IdSimple, "Simple (Standard)")
    };

    private readonly FluentTheme _fluentTheme = new();
    private readonly SimpleTheme _simpleTheme = new();
    private readonly StyleInclude _customStyles;

    public ThemeService()
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "CashflowSimulator.Desktop";
        var uri = new Uri($"avares://{assemblyName}/Common/Themes/Default.axaml");
        _customStyles = new StyleInclude(uri) { Source = uri };
    }

    /// <inheritdoc />
    public IReadOnlyList<ThemeOption> GetAvailableThemes() => Themes;

    /// <inheritdoc />
    public string GetDefaultThemeId() => IdFluent;

    /// <inheritdoc />
    public void ApplyTheme(string? themeId)
    {
        var app = Application.Current;
        if (app is null)
            return;

        var id = string.IsNullOrWhiteSpace(themeId) ? GetDefaultThemeId() : themeId.Trim();

        // Theme-Wechsel verzögern, damit offene Popups (z. B. ComboBox-Dropdown) zuerst
        // geschlossen werden. Sonst: InvalidOperationException "already has a visual parent".
        Dispatcher.UIThread.Post(() => ApplyThemeCore(app, id), DispatcherPriority.Loaded);
    }

    private void ApplyThemeCore(Application app, string id)
    {
        if (Application.Current is null)
            return;

        app.Styles.Clear();

        switch (id)
        {
            case IdSimple:
                app.Styles.Add(_simpleTheme);
                app.Styles.Add(_customStyles);
                break;
            case IdFluent:
            default:
                app.Styles.Add(_fluentTheme);
                app.Styles.Add(_customStyles);
                break;
        }
    }
}
