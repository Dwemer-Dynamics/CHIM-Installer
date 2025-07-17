using CHIMInstaller.Core.Interfaces;
using CHIMInstaller.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;

namespace CHIMInstaller.Core.Services;

public class PrerequisiteService : IPrerequisiteService
{
    private readonly ILogger<PrerequisiteService> _logger;
    private readonly HttpClient _httpClient;

    private readonly string[] RequiredFeatures = new[]
    {
        "VirtualMachinePlatform",
        "HypervisorPlatform", 
        "Microsoft-Windows-Subsystem-Linux"
    };

    public PrerequisiteService(ILogger<PrerequisiteService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<InstallationResult> CheckAndInstallVisualCppAsync(IProgress<string> progress)
    {
        try
        {
            progress?.Report("Checking for Microsoft Visual C++ Redistributable package...");
            _logger.LogInformation("Checking for Visual C++ Redistributable...");

            const string latestVersion = "14.36.32532.0";
            const string vcKeyPath = @"HKLM\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";

            var isInstalled = await IsVisualCppInstalledAsync();
            var installedVersion = GetInstalledVisualCppVersion();

            if (isInstalled && !string.IsNullOrEmpty(installedVersion))
            {
                progress?.Report($"Installed Visual C++ Redistributable version: {installedVersion}");
                
                if (Version.TryParse(installedVersion, out var installed) && 
                    Version.TryParse(latestVersion, out var latest) &&
                    installed >= latest)
                {
                    progress?.Report("The installed version is up to date.");
                    return new InstallationResult
                    {
                        IsSuccess = true,
                        Message = "Visual C++ Redistributable is already up to date."
                    };
                }
                else
                {
                    progress?.Report($"Installed version is outdated. Updating to version {latestVersion}...");
                }
            }
            else
            {
                progress?.Report($"Microsoft Visual C++ Redistributable is not installed. Installing version {latestVersion}...");
            }

            // Download and install
            const string downloadUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
            var outputPath = Path.Combine(Path.GetTempPath(), "vc_redist.x64.exe");

            try
            {
                progress?.Report("Downloading Visual C++ Redistributable...");
                _logger.LogInformation($"Downloading Visual C++ Redistributable from {downloadUrl}");

                using var response = await _httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                await using var fileStream = File.Create(outputPath);
                await response.Content.CopyToAsync(fileStream);

                progress?.Report($"Downloaded Visual C++ Redistributable to {outputPath}");
                _logger.LogInformation("Downloaded Visual C++ Redistributable successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download Visual C++ Redistributable");
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "Failed to download Visual C++ Redistributable",
                    ErrorDetails = ex.Message
                };
            }

            try
            {
                progress?.Report("Installing Microsoft Visual C++ Redistributable...");
                _logger.LogInformation("Installing Visual C++ Redistributable...");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = outputPath,
                        Arguments = "/quiet /norestart",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    progress?.Report($"Microsoft Visual C++ Redistributable has been installed or updated to version {latestVersion}.");
                    _logger.LogInformation("Visual C++ Redistributable installed successfully");
                    
                    return new InstallationResult
                    {
                        IsSuccess = true,
                        Message = "Visual C++ Redistributable installed successfully",
                        ExitCode = process.ExitCode
                    };
                }
                else
                {
                    _logger.LogError($"Visual C++ Redistributable installation failed with exit code {process.ExitCode}");
                    return new InstallationResult
                    {
                        IsSuccess = false,
                        Message = "Visual C++ Redistributable installation failed",
                        ExitCode = process.ExitCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing Visual C++ Redistributable");
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "Failed to install Visual C++ Redistributable",
                    ErrorDetails = ex.Message
                };
            }
            finally
            {
                // Clean up downloaded file
                try
                {
                    if (File.Exists(outputPath))
                        File.Delete(outputPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckAndInstallVisualCppAsync");
            return new InstallationResult
            {
                IsSuccess = false,
                Message = "Unexpected error during Visual C++ installation",
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<InstallationResult> EnableWindowsFeatureAsync(string featureName, IProgress<string> progress)
    {
        try
        {
            progress?.Report($"Checking feature '{featureName}'...");
            
            var isEnabled = await IsWindowsFeatureEnabledAsync(featureName);
            
            if (isEnabled)
            {
                progress?.Report($"Feature '{featureName}' is already enabled.");
                return new InstallationResult
                {
                    IsSuccess = true,
                    Message = $"Feature '{featureName}' is already enabled."
                };
            }

            progress?.Report($"Enabling feature '{featureName}'...");
            _logger.LogInformation($"Enabling Windows feature: {featureName}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dism.exe",
                    Arguments = $"/Online /Enable-Feature /FeatureName:{featureName} /All /NoRestart",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                progress?.Report($"Feature '{featureName}' enabled successfully.");
                _logger.LogInformation($"Windows feature '{featureName}' enabled successfully");
                
                return new InstallationResult
                {
                    IsSuccess = true,
                    Message = $"Feature '{featureName}' enabled successfully",
                    ExitCode = process.ExitCode,
                    RequiresRestart = output.Contains("restart", StringComparison.OrdinalIgnoreCase)
                };
            }
            else
            {
                _logger.LogError($"Failed to enable Windows feature '{featureName}'. Exit code: {process.ExitCode}, Error: {error}");
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = $"Failed to enable feature '{featureName}'",
                    ErrorDetails = $"Exit code: {process.ExitCode}\nOutput: {output}\nError: {error}",
                    ExitCode = process.ExitCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error enabling Windows feature '{featureName}'");
            return new InstallationResult
            {
                IsSuccess = false,
                Message = $"Error enabling feature '{featureName}'",
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<InstallationResult> EnableAllRequiredFeaturesAsync(IProgress<string> progress)
    {
        try
        {
            progress?.Report("Enabling required Windows features...");
            _logger.LogInformation("Enabling all required Windows features");

            var results = new List<InstallationResult>();
            bool anyRequiresRestart = false;

            foreach (var feature in RequiredFeatures)
            {
                var result = await EnableWindowsFeatureAsync(feature, progress);
                results.Add(result);
                
                if (result.RequiresRestart)
                    anyRequiresRestart = true;
            }

            var failedFeatures = results.Where(r => !r.IsSuccess).ToList();

            if (failedFeatures.Any())
            {
                var failedNames = RequiredFeatures.Where((f, i) => !results[i].IsSuccess).ToArray();
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = $"Failed to enable {failedFeatures.Count} Windows feature(s): {string.Join(", ", failedNames)}",
                    ErrorDetails = string.Join("\n", failedFeatures.Select(f => f.ErrorDetails)),
                    RequiresRestart = anyRequiresRestart
                };
            }

            progress?.Report("All specified Windows features are installed.");
            if (anyRequiresRestart)
            {
                progress?.Report("A restart may be required to complete the feature installation.");
            }

            return new InstallationResult
            {
                IsSuccess = true,
                Message = "All required Windows features enabled successfully",
                RequiresRestart = anyRequiresRestart
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling Windows features");
            return new InstallationResult
            {
                IsSuccess = false,
                Message = "Error enabling Windows features",
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<bool> IsVisualCppInstalledAsync()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
            return key?.GetValue("Version") != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsWindowsFeatureEnabledAsync(string featureName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dism.exe",
                    Arguments = $"/Online /Get-FeatureInfo /FeatureName:{featureName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0 && output.Contains("State : Enabled", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private string? GetInstalledVisualCppVersion()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
            return key?.GetValue("Version")?.ToString();
        }
        catch
        {
            return null;
        }
    }
} 