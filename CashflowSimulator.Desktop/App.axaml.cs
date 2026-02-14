using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CashflowSimulator.Desktop
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var themeService = CompositionRoot.Services.GetRequiredService<IThemeService>();
                var projectService = CompositionRoot.Services.GetRequiredService<ICurrentProjectService>();
                var themeId = projectService.Current?.UiSettings.SelectedThemeId;
                themeService.ApplyTheme(string.IsNullOrWhiteSpace(themeId) ? themeService.GetDefaultThemeId() : themeId);

                desktop.MainWindow = CompositionRoot.Services.GetRequiredService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}