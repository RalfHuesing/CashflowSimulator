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
using CashflowSimulator.Desktop.Features.LifecyclePhases;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Engine.Services;
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

                // Shell / Main-Feature (Func<T> wird von MS.DI nicht automatisch bereitgestellt)
                services.AddTransient<NavigationViewModel>();
                services.AddTransient<MetaEditViewModel>();
                services.AddSingleton<Func<MetaEditViewModel>>(sp => () => sp.GetRequiredService<MetaEditViewModel>());
                services.AddTransient<EckdatenViewModel>();
                services.AddSingleton<Func<EckdatenViewModel>>(sp => () => sp.GetRequiredService<EckdatenViewModel>());
                services.AddTransient<MarktdatenViewModel>();
                services.AddSingleton<Func<MarktdatenViewModel>>(sp => () => sp.GetRequiredService<MarktdatenViewModel>());
                services.AddTransient<KorrelationenViewModel>();
                services.AddSingleton<Func<KorrelationenViewModel>>(sp => () => sp.GetRequiredService<KorrelationenViewModel>());
                services.AddTransient<AssetClassesViewModel>();
                services.AddSingleton<Func<AssetClassesViewModel>>(sp => () => sp.GetRequiredService<AssetClassesViewModel>());
                services.AddTransient<PortfolioViewModel>();
                services.AddSingleton<Func<PortfolioViewModel>>(sp => () => sp.GetRequiredService<PortfolioViewModel>());
                services.AddTransient<TransactionsViewModel>();
                services.AddSingleton<Func<TransactionsViewModel>>(sp => () => sp.GetRequiredService<TransactionsViewModel>());
                services.AddTransient<TaxProfilesViewModel>();
                services.AddSingleton<Func<TaxProfilesViewModel>>(sp => () => sp.GetRequiredService<TaxProfilesViewModel>());
                services.AddTransient<StrategyProfilesViewModel>();
                services.AddSingleton<Func<StrategyProfilesViewModel>>(sp => () => sp.GetRequiredService<StrategyProfilesViewModel>());
                services.AddTransient<LifecyclePhasesViewModel>();
                services.AddSingleton<Func<LifecyclePhasesViewModel>>(sp => () => sp.GetRequiredService<LifecyclePhasesViewModel>());
                services.AddTransient<SettingsViewModel>();
                services.AddSingleton<Func<SettingsViewModel>>(sp => () => sp.GetRequiredService<SettingsViewModel>());
                services.AddSingleton<Func<CashflowType, CashflowStreamsViewModel>>(sp => type =>
                    new CashflowStreamsViewModel(sp.GetRequiredService<ICurrentProjectService>(), sp.GetRequiredService<IHelpProvider>(), type));
                services.AddSingleton<Func<CashflowType, CashflowEventsViewModel>>(sp => type =>
                    new CashflowEventsViewModel(sp.GetRequiredService<ICurrentProjectService>(), sp.GetRequiredService<IHelpProvider>(), type));
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
