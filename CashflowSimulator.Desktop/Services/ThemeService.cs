using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Wendet zur Laufzeit Fluent- oder Simple-Basis-Theme an.
/// Default.axaml (StyleInclude) kommt aus App.axaml und bleibt unberührt; nur das Basis-Theme wird getauscht (Swap statt Clear).
/// </summary>
public sealed class ThemeService : IThemeService
{
    public const string IdFluent = "Fluent";
    public const string IdSimple = "Simple";

    private static readonly IReadOnlyList<ThemeOption> Themes = new[]
    {
        new ThemeOption(IdFluent, "Fluent (Standard)"),
        new ThemeOption(IdSimple, "Simple (Standard)")
    };

    private IStyle? _currentBaseTheme;

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

        Dispatcher.UIThread.Post(() => ApplyThemeCore(app, id), DispatcherPriority.Normal);
    }

    private void ApplyThemeCore(Application app, string id)
    {
        if (Application.Current is null)
            return;

        IStyle newTheme = id switch
        {
            IdSimple => new SimpleTheme(),
            IdFluent or _ => new FluentTheme()
        };

        if (_currentBaseTheme is not null && app.Styles.Contains(_currentBaseTheme))
        {
            app.Styles.Remove(_currentBaseTheme);
        }
        else
        {
            for (var i = app.Styles.Count - 1; i >= 0; i--)
            {
                var style = app.Styles[i];
                if (style is FluentTheme or SimpleTheme)
                {
                    app.Styles.RemoveAt(i);
                }
            }
        }

        app.Styles.Insert(0, newTheme);
        _currentBaseTheme = newTheme;
    }
}
