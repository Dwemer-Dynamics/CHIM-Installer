using CHIMInstaller.Core.Models;

namespace CHIMInstaller.Core.Interfaces;

public interface IPrerequisiteService
{
    Task<InstallationResult> CheckAndInstallVisualCppAsync(IProgress<string> progress);
    Task<InstallationResult> EnableWindowsFeatureAsync(string featureName, IProgress<string> progress);
    Task<InstallationResult> EnableAllRequiredFeaturesAsync(IProgress<string> progress);
    Task<bool> IsVisualCppInstalledAsync();
    Task<bool> IsWindowsFeatureEnabledAsync(string featureName);
} 