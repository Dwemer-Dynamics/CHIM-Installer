using CHIMInstaller.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CHIMInstaller.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private InstallationConfiguration _configuration;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _configuration = new InstallationConfiguration
        {
            InstallationPath = @"C:\CHIM",
            SelectedComponents = new List<string> { "Core", "HerikaServer", "AIAgent" }
        };
    }

    public InstallationConfiguration Configuration => _configuration;

    public void SetInstallationPath(string path)
    {
        _configuration.InstallationPath = path;
    }

    public void SetSelectedComponents(List<string> components)
    {
        _configuration.SelectedComponents = components;
    }

    public async Task<bool> SaveConfigurationAsync()
    {
        return true;
    }

    public async Task<bool> LoadConfigurationAsync()
    {
        return true;
    }

    public ValidationResult ValidateConfiguration()
    {
        return new ValidationResult { IsValid = true };
    }

    public string GetDefaultInstallationPath()
    {
        return @"C:\CHIM";
    }

    public List<InstallationComponent> GetAvailableComponents()
    {
        return new List<InstallationComponent>
        {
            new() { Name = "Core", DisplayName = "CHIM Core", IsRequired = true, IsSelected = true },
            new() { Name = "HerikaServer", DisplayName = "Herika Server", IsRequired = true, IsSelected = true },
            new() { Name = "AIAgent", DisplayName = "AI Agent", IsRequired = true, IsSelected = true }
        };
    }

    public long EstimateInstallationSize()
    {
        return 3L * 1024 * 1024 * 1024; // 3 GB
    }
} 