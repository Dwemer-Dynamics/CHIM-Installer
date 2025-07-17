using CHIMInstaller.Core.Models;

namespace CHIMInstaller.Core.Interfaces;

public interface IConfigurationService
{
    Task<InstallationResult> RunDistroInstallAsync(string chimDirectory, IProgress<string> progress);
    Task<InstallationResult> RunCudaInstallAsync(string chimDirectory, IProgress<string> progress);
    Task<bool> ShouldRestartSystemAsync();
    Task<InstallationResult> CompleteInstallationAsync(string chimDirectory, IProgress<string> progress);
} 