using CHIMInstaller.Models;

namespace CHIMInstaller.Services;

public interface IConfigurationService
{
    /// <summary>
    /// Gets the current installation configuration
    /// </summary>
    InstallationConfiguration Configuration { get; }

    /// <summary>
    /// Sets the installation path
    /// </summary>
    /// <param name="path">Installation path</param>
    void SetInstallationPath(string path);

    /// <summary>
    /// Sets the selected components to install
    /// </summary>
    /// <param name="components">List of component names</param>
    void SetSelectedComponents(List<string> components);

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    /// <returns>True if saved successfully</returns>
    Task<bool> SaveConfigurationAsync();

    /// <summary>
    /// Loads configuration from disk
    /// </summary>
    /// <returns>True if loaded successfully</returns>
    Task<bool> LoadConfigurationAsync();

    /// <summary>
    /// Validates the current configuration
    /// </summary>
    /// <returns>Validation result</returns>
    ValidationResult ValidateConfiguration();

    /// <summary>
    /// Gets the default installation path
    /// </summary>
    /// <returns>Default installation path</returns>
    string GetDefaultInstallationPath();

    /// <summary>
    /// Gets available installation components
    /// </summary>
    /// <returns>List of available components</returns>
    List<InstallationComponent> GetAvailableComponents();

    /// <summary>
    /// Estimates the total installation size
    /// </summary>
    /// <returns>Estimated size in bytes</returns>
    long EstimateInstallationSize();
} 