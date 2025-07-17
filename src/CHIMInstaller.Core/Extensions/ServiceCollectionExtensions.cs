using CHIMInstaller.Core.Interfaces;
using CHIMInstaller.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CHIMInstaller.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Register all core services
        services.AddScoped<ISystemCheckService, SystemCheckService>();
        services.AddScoped<IPrerequisiteService, PrerequisiteService>();
        services.AddScoped<IDownloadService, DownloadService>();
        services.AddScoped<IExtractionService, ExtractionService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();

        // Register HttpClient for download services
        services.AddHttpClient<IDownloadService, DownloadService>();
        services.AddHttpClient<IPrerequisiteService, PrerequisiteService>();

        return services;
    }
} 