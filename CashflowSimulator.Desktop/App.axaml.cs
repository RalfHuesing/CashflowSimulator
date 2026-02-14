using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CashflowSimulator.Contracts.Interfaces;
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
                var projectService = CompositionRoot.Services.GetRequiredService<ICurrentProjectService>();
                var defaultProvider = CompositionRoot.Services.GetRequiredService<IDefaultProjectProvider>();
                projectService.SetCurrent(defaultProvider.CreateDefault());

                desktop.MainWindow = CompositionRoot.Services.GetRequiredService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}