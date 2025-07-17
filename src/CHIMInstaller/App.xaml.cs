using CHIMInstaller.Core.Extensions;
using CHIMInstaller.Services;
using CHIMInstaller.ViewModels;
using CHIMInstaller.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Reflection;
using System.Windows;

namespace CHIMInstaller;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    public App()
    {
        DispatcherUnhandledException += Application_DispatcherUnhandledException;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CHIMInstaller", "Logs", "installer.log");
        
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();

        try
        {
            Log.Information("Starting CHIM Installer...");

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Information("Configuration loaded successfully");

            // Build host with dependency injection
            _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, configuration);
                })
                .UseSerilog()
                .Build();

            Log.Information("Host created, starting services...");
            await _host.StartAsync();
            Log.Information("Host started successfully");

            // Show main window
            Log.Information("Creating main window...");
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            Log.Information("Main window created, showing...");
            mainWindow.Show();
            Log.Information("Main window shown successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            MessageBox.Show($"Failed to start CHIM Installer: {ex.Message}\n\nDetails: {ex.ToString()}", "Startup Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.AddSingleton(configuration);

        // Core Services from CHIMInstaller.Core
        services.AddCoreServices();
        
        // UI Services (local to WPF app)
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<WelcomeViewModel>();
        services.AddTransient<SystemCheckViewModel>();
        services.AddTransient<PrerequisitesViewModel>();
        services.AddTransient<InstallationPathViewModel>();
        services.AddTransient<DownloadViewModel>();
        services.AddTransient<InstallationProgressViewModel>();
        services.AddTransient<CompletionViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddTransient<WelcomeView>();
        services.AddTransient<SystemCheckView>();
        services.AddTransient<PrerequisitesView>();
        services.AddTransient<InstallationPathView>();
        services.AddTransient<DownloadView>();
        services.AddTransient<InstallationProgressView>();
        services.AddTransient<CompletionView>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception occurred");
        
        MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}\n\nPlease check the log files for more details.", 
            "CHIM Installer Error", MessageBoxButton.OK, MessageBoxImage.Error);
        
        e.Handled = true;
        Shutdown(1);
    }
} 