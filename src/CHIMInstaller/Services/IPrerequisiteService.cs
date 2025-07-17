using CHIMInstaller.Models;

namespace CHIMInstaller.Services;

public interface IPrerequisiteService
{
    /// <summary>
    /// Installs the latest Visual C++ Redistributable
    /// </summary>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Installation result</returns>
    Task<InstallationResult> InstallVisualCppRedistributableAsync(
        IProgress<InstallationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Installs 7-Zip if not already installed
    /// </summary>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Installation result</returns>
    Task<InstallationResult> Install7ZipAsync(
        IProgress<InstallationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables required Windows features
    /// </summary>
    /// <param name="features">List of features to enable</param>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Installation result</returns>
    Task<InstallationResult> EnableWindowsFeaturesAsync(
        List<string> features,
        IProgress<InstallationProgress>? progress = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if Visual C++ Redistributable is installed
    /// </summary>
    /// <returns>True if installed</returns>
    bool IsVisualCppRedistributableInstalled();

    /// <summary>
    /// Checks if 7-Zip is installed and accessible
    /// </summary>
    /// <returns>True if installed and accessible</returns>
    bool Is7ZipInstalled();

    /// <summary>
    /// Gets the status of required Windows features
    /// </summary>
    /// <returns>Dictionary of feature names and their installation status</returns>
    Task<Dictionary<string, bool>> GetWindowsFeaturesStatusAsync();
} 