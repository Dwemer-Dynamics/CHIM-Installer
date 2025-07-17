using CHIMInstaller.Models;

namespace CHIMInstaller.Services;

public interface ISystemCheckService
{
    /// <summary>
    /// Performs a comprehensive system check to verify all requirements
    /// </summary>
    /// <returns>System check results</returns>
    Task<SystemCheckResult> PerformSystemCheckAsync();

    /// <summary>
    /// Checks if the application is running with administrator privileges
    /// </summary>
    /// <returns>True if running as administrator</returns>
    bool IsRunningAsAdministrator();

    /// <summary>
    /// Checks if virtualization is enabled in the BIOS
    /// </summary>
    /// <returns>True if virtualization is enabled</returns>
    bool IsVirtualizationEnabled();

    /// <summary>
    /// Checks available disk space on the specified drive
    /// </summary>
    /// <param name="driveLetter">Drive letter to check</param>
    /// <returns>Available space in bytes</returns>
    long GetAvailableDiskSpace(string driveLetter);

    /// <summary>
    /// Checks if the current Windows version is supported
    /// </summary>
    /// <returns>True if Windows version is supported</returns>
    bool IsWindowsVersionSupported();

    /// <summary>
    /// Checks internet connectivity
    /// </summary>
    /// <returns>True if internet connection is available</returns>
    Task<bool> IsInternetAvailableAsync();

    /// <summary>
    /// Checks if required Windows features are enabled
    /// </summary>
    /// <returns>Dictionary of feature names and their status</returns>
    Task<Dictionary<string, bool>> CheckWindowsFeaturesAsync();
} 