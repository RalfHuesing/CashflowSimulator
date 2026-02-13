using System;
using Avalonia;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Main;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CashflowSimulator.Desktop
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .CreateLogger();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));

            // App-weite Services (plattformunabhängig)
            services.AddSingleton<AvaloniaFileDialogService>();
            services.AddSingleton<IFileDialogService>(sp => sp.GetRequiredService<AvaloniaFileDialogService>());
            services.AddSingleton<IStorageService<SimulationProjectDto>, JsonFileStorageService<SimulationProjectDto>>();
            services.AddSingleton<MetaEditDialogService>();
            services.AddSingleton<IMetaEditDialogService>(sp => sp.GetRequiredService<MetaEditDialogService>());

            // Shell / Main-Feature
            services.AddTransient<NavigationViewModel>();
            services.AddTransient<MainShellViewModel>();
            services.AddTransient<MainWindow>();

            var serviceProvider = services.BuildServiceProvider();
            CompositionRoot.Services = serviceProvider;

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
