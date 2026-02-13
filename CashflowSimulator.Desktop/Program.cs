using System;
using System.IO;
using Avalonia;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.Features.Main;
using CashflowSimulator.Desktop.Features.Main.Navigation;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Services;
using CashflowSimulator.Engine.Services;
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
                services.AddSingleton<IDefaultProjectProvider, DefaultProjectProvider>();
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
