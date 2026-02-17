using System;
using System.Globalization;
using System.IO;
using Avalonia;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.CashflowEvents;
using CashflowSimulator.Desktop.Features.CashflowStreams;
using CashflowSimulator.Desktop.Features.Eckdaten;
using CashflowSimulator.Desktop.Features.Main;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Features.Marktdaten;
using CashflowSimulator.Desktop.Features.Korrelationen;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Features.Portfolio;
using CashflowSimulator.Desktop.Features.Settings;
using CashflowSimulator.Desktop.Features.TaxProfiles;
using CashflowSimulator.Desktop.Features.StrategyProfiles;
using CashflowSimulator.Desktop.Features.AllocationProfiles;
using CashflowSimulator.Desktop.Features.LifecyclePhases;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Engine.Services.Defaults;
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
            var culture = new CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            var logPath = Path.Combine(logDirectory, "cashflow-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Application starting");
                var services = new ServiceCollection();
                services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));

                // App-weite Services (plattformunabhängig)
                services.AddSingleton<AvaloniaFileDialogService>();
                services.AddSingleton<IFileDialogService>(sp => sp.GetRequiredService<AvaloniaFileDialogService>());
                services.AddSingleton<IStorageService<SimulationProjectDto>, JsonFileStorageService<SimulationProjectDto>>();
                services.AddSingleton<ISimulationTimeService, SimulationTimeService>();
                services.AddSingleton<IMarketDataService, MarketDataService>();
                services.AddSingleton<ICashflowDefaultService, CashflowDefaultService>();
                services.AddSingleton<IPortfolioDefaultService, PortfolioDefaultService>();
                services.AddSingleton<IDefaultProjectProvider, DefaultProjectProvider>();
                services.AddSingleton<ICurrentProjectService, CurrentProjectService>();
                services.AddSingleton<IHelpProvider, HelpProvider>();

                // Navigation Service
                services.AddSingleton<INavigationService, NavigationService>();

                // ViewModels (als Transient registriert, werden über NavigationService aufgelöst)
                services.AddTransient<NavigationViewModel>();
                services.AddTransient<MetaEditViewModel>();
                services.AddTransient<EckdatenViewModel>();
                services.AddTransient<MarktdatenViewModel>();
                services.AddTransient<KorrelationenViewModel>();
                services.AddTransient<AssetClassesViewModel>();
                services.AddTransient<PortfolioViewModel>();
                services.AddTransient<TransactionsViewModel>();
                services.AddTransient<TaxProfilesViewModel>();
                services.AddTransient<StrategyProfilesViewModel>();
                services.AddTransient<AllocationProfilesViewModel>();
                services.AddTransient<LifecyclePhasesViewModel>();
                services.AddTransient<SettingsViewModel>();
                
                // Shell & Main Window
                services.AddTransient<MainShellViewModel>();
                services.AddTransient<MainWindow>();

                var serviceProvider = services.BuildServiceProvider();
                CompositionRoot.Services = serviceProvider;

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
