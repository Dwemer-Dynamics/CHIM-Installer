using CHIMInstaller.Core.Interfaces;
using CHIMInstaller.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CHIMInstaller.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private bool _restartRequired = false;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
    }

    public async Task<InstallationResult> RunDistroInstallAsync(string chimDirectory, IProgress<string> progress)
    {
        try
        {
            progress?.Report("Looking for distribution installation script...");
            _logger.LogInformation("Starting distribution installation");

            var batFile = Path.Combine(chimDirectory, "DwemerAI4Skyrim3", "1) INSTALL Distro.bat");
            
            if (!File.Exists(batFile))
            {
                _logger.LogWarning($"Distribution install script not found at: {batFile}");
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "Distribution install script not found",
                    ErrorDetails = $"Expected file: {batFile}"
                };
            }

            progress?.Report($"Running '1) INSTALL Distro.bat'...");
            _logger.LogInformation($"Running distribution install script: {batFile}");

            var workingDirectory = Path.GetDirectoryName(batFile);
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = batFile,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false // Show window for batch file execution
                }
            };

            var outputData = new List<string>();
            var errorData = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputData.Add(e.Data);
                    progress?.Report($"Distro Install: {e.Data}");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorData.Add(e.Data);
                    _logger.LogWarning($"Distro Install stderr: {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var output = string.Join("\n", outputData);
            var error = string.Join("\n", errorData);

            if (process.ExitCode == 0)
            {
                progress?.Report("Distribution installation completed successfully.");
                _logger.LogInformation("Distribution installation completed successfully");
                
                return new InstallationResult
                {
                    IsSuccess = true,
                    Message = "Distribution installation completed successfully",
                    ExitCode = process.ExitCode
                };
            }
            else
            {
                _logger.LogError($"Distribution installation failed with exit code {process.ExitCode}");
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "Distribution installation failed",
                    ErrorDetails = $"Exit code: {process.ExitCode}\nOutput: {output}\nError: {error}",
                    ExitCode = process.ExitCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running distribution installation");
            return new InstallationResult
            {
                IsSuccess = false,
                Message = "Error running distribution installation",
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<InstallationResult> RunCudaInstallAsync(string chimDirectory, IProgress<string> progress)
    {
        try
        {
            progress?.Report("Looking for CUDA installation script...");
            _logger.LogInformation("Starting CUDA installation");

            var batFile = Path.Combine(chimDirectory, "DwemerAI4Skyrim3", "Tools", "Components", "NVIDIA GPU Components", "1) REQUIRED CUDA INSTALL ME!.bat");
            
            if (!File.Exists(batFile))
            {
                _logger.LogWarning($"CUDA install script not found at: {batFile}");
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "CUDA install script not found",
                    ErrorDetails = $"Expected file: {batFile}"
                };
            }

            progress?.Report($"Running '1) REQUIRED CUDA INSTALL ME!.bat'...");
            _logger.LogInformation($"Running CUDA install script: {batFile}");

            var workingDirectory = Path.GetDirectoryName(batFile);
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = batFile,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false // Show window for batch file execution
                }
            };

            var outputData = new List<string>();
            var errorData = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputData.Add(e.Data);
                    progress?.Report($"CUDA Install: {e.Data}");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorData.Add(e.Data);
                    _logger.LogWarning($"CUDA Install stderr: {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var output = string.Join("\n", outputData);
            var error = string.Join("\n", errorData);

            if (process.ExitCode == 0)
            {
                progress?.Report("CUDA installation completed successfully.");
                _logger.LogInformation("CUDA installation completed successfully");
                
                return new InstallationResult
                {
                    IsSuccess = true,
                    Message = "CUDA installation completed successfully",
                    ExitCode = process.ExitCode
                };
            }
            else
            {
                _logger.LogError($"CUDA installation failed with exit code {process.ExitCode}");
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "CUDA installation failed",
                    ErrorDetails = $"Exit code: {process.ExitCode}\nOutput: {output}\nError: {error}",
                    ExitCode = process.ExitCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running CUDA installation");
            return new InstallationResult
            {
                IsSuccess = false,
                Message = "Error running CUDA installation",
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<bool> ShouldRestartSystemAsync()
    {
        return _restartRequired;
    }

    public async Task<InstallationResult> CompleteInstallationAsync(string chimDirectory, IProgress<string> progress)
    {
        try
        {
            progress?.Report("Completing installation...");
            _logger.LogInformation("Completing CHIM AI installation");

            var results = new List<InstallationResult>();

            // Run distro installation
            progress?.Report("Step 1: Running distribution installation...");
            var distroResult = await RunDistroInstallAsync(chimDirectory, progress);
            results.Add(distroResult);

            if (!distroResult.IsSuccess)
            {
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "Failed during distribution installation",
                    ErrorDetails = distroResult.ErrorDetails
                };
            }

            // Pause equivalent - let user know to wait
            progress?.Report("Please let the installation complete. Do not close any windows that may appear.");
            await Task.Delay(2000); // Brief pause to let user see the message

            // Run CUDA installation
            progress?.Report("Step 2: Running CUDA installation...");
            var cudaResult = await RunCudaInstallAsync(chimDirectory, progress);
            results.Add(cudaResult);

            if (!cudaResult.IsSuccess)
            {
                return new InstallationResult
                {
                    IsSuccess = false,
                    Message = "Failed during CUDA installation",
                    ErrorDetails = cudaResult.ErrorDetails
                };
            }

            progress?.Report("CHIM AI installation completed successfully!");
            _logger.LogInformation("CHIM AI installation completed successfully");

            return new InstallationResult
            {
                IsSuccess = true,
                Message = "CHIM AI installation completed successfully. The automated installation steps have finished.",
                RequiresRestart = _restartRequired
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing installation");
            return new InstallationResult
            {
                IsSuccess = false,
                Message = "Error completing installation",
                ErrorDetails = ex.Message
            };
        }
    }

    /// <summary>
    /// Marks that a system restart will be required
    /// </summary>
    public void SetRestartRequired()
    {
        _restartRequired = true;
        _logger.LogInformation("System restart marked as required");
    }

    /// <summary>
    /// Initiates a system restart (with user confirmation)
    /// </summary>
    public async Task<InstallationResult> RestartSystemAsync(IProgress<string> progress)
    {
        try
        {
            progress?.Report("Restarting the computer...");
            _logger.LogInformation("Initiating system restart");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 10 /c \"CHIM AI installation requires a system restart to complete.\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return new InstallationResult
            {
                IsSuccess = true,
                Message = "System restart initiated. The computer will restart in 10 seconds.",
                ExitCode = process.ExitCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating system restart");
            return new InstallationResult
            {
                IsSuccess = false,
                Message = "Failed to initiate system restart",
                ErrorDetails = ex.Message
            };
        }
    }
} 