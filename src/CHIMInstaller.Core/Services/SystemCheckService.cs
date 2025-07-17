using CHIMInstaller.Core.Interfaces;
using CHIMInstaller.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;

namespace CHIMInstaller.Core.Services;

public class SystemCheckService : ISystemCheckService
{
    private readonly ILogger<SystemCheckService> _logger;

    public SystemCheckService(ILogger<SystemCheckService> logger)
    {
        _logger = logger;
    }

    public async Task<SystemCheckResult> CheckVirtualizationAsync()
    {
        try
        {
            _logger.LogInformation("Checking if Virtualization is enabled in the BIOS...");
            
            bool hyperVSupport = false;
            
            // Check using WMI
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    var hypervisorPresent = obj["HypervisorPresent"];
                    if (hypervisorPresent != null && (bool)hypervisorPresent)
                    {
                        hyperVSupport = true;
                        break;
                    }
                }
            }

            if (hyperVSupport)
            {
                _logger.LogInformation("Virtualization is enabled in the BIOS.");
                return new SystemCheckResult
                {
                    IsSuccess = true,
                    Message = "Virtualization is enabled in the BIOS.",
                    CheckType = SystemCheckType.Virtualization
                };
            }
            else
            {
                _logger.LogWarning("Virtualization is NOT enabled in the BIOS.");
                return new SystemCheckResult
                {
                    IsSuccess = false,
                    Message = "Virtualization is NOT enabled in the BIOS.",
                    Details = "Some features may not function correctly. Please enable Virtualization (Intel VT-x or AMD-V) in your system's BIOS settings if needed.",
                    CheckType = SystemCheckType.Virtualization
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking virtualization status");
            return new SystemCheckResult
            {
                IsSuccess = false,
                Message = "Failed to check virtualization status",
                Details = ex.Message,
                CheckType = SystemCheckType.Virtualization
            };
        }
    }

    public async Task<SystemCheckResult> CheckDiskSpaceAsync(string targetDrive, long requiredSpaceBytes)
    {
        try
        {
            _logger.LogInformation($"Checking disk space on drive {targetDrive}...");
            
            var driveInfo = new DriveInfo(targetDrive);
            
            if (!driveInfo.IsReady)
            {
                return new SystemCheckResult
                {
                    IsSuccess = false,
                    Message = $"Drive {targetDrive} is not ready or does not exist.",
                    CheckType = SystemCheckType.DiskSpace
                };
            }

            long availableSpace = driveInfo.AvailableFreeSpace;
            double requiredGB = requiredSpaceBytes / (1024.0 * 1024.0 * 1024.0);
            double availableGB = availableSpace / (1024.0 * 1024.0 * 1024.0);

            if (availableSpace >= requiredSpaceBytes)
            {
                return new SystemCheckResult
                {
                    IsSuccess = true,
                    Message = $"Sufficient disk space available: {availableGB:F1} GB free (requires {requiredGB:F1} GB)",
                    CheckType = SystemCheckType.DiskSpace
                };
            }
            else
            {
                return new SystemCheckResult
                {
                    IsSuccess = false,
                    Message = $"Insufficient disk space: {availableGB:F1} GB free (requires {requiredGB:F1} GB)",
                    CheckType = SystemCheckType.DiskSpace
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking disk space");
            return new SystemCheckResult
            {
                IsSuccess = false,
                Message = "Failed to check disk space",
                Details = ex.Message,
                CheckType = SystemCheckType.DiskSpace
            };
        }
    }

    public async Task<SystemCheckResult> CheckAdminRightsAsync()
    {
        try
        {
            _logger.LogInformation("Checking administrator privileges...");
            
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

                if (isAdmin)
                {
                    return new SystemCheckResult
                    {
                        IsSuccess = true,
                        Message = "Running with administrator privileges.",
                        CheckType = SystemCheckType.AdminRights
                    };
                }
                else
                {
                    return new SystemCheckResult
                    {
                        IsSuccess = false,
                        Message = "Administrator privileges required.",
                        Details = "Please run the installer as administrator to continue.",
                        CheckType = SystemCheckType.AdminRights
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking administrator privileges");
            return new SystemCheckResult
            {
                IsSuccess = false,
                Message = "Failed to check administrator privileges",
                Details = ex.Message,
                CheckType = SystemCheckType.AdminRights
            };
        }
    }

    public async Task<SystemCheckResult> CheckSevenZipAsync()
    {
        try
        {
            _logger.LogInformation("Checking for 7-Zip installation...");
            
            var sevenZipPath = await Find7ZipExecutableAsync();
            
            if (!string.IsNullOrEmpty(sevenZipPath))
            {
                return new SystemCheckResult
                {
                    IsSuccess = true,
                    Message = $"7-Zip found at: {sevenZipPath}",
                    Details = sevenZipPath,
                    CheckType = SystemCheckType.SevenZip
                };
            }
            else
            {
                return new SystemCheckResult
                {
                    IsSuccess = false,
                    Message = "7-Zip not found.",
                    Details = "7-Zip is required to extract the mod files. Please install 7-Zip from https://www.7-zip.org/",
                    CheckType = SystemCheckType.SevenZip
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 7-Zip availability");
            return new SystemCheckResult
            {
                IsSuccess = false,
                Message = "Failed to check 7-Zip availability",
                Details = ex.Message,
                CheckType = SystemCheckType.SevenZip
            };
        }
    }

    public async Task<SystemCheckResult> RunAllChecksAsync(string targetDrive, long requiredSpaceBytes)
    {
        try
        {
            _logger.LogInformation("Running all system checks...");
            
            var checks = new List<SystemCheckResult>
            {
                await CheckAdminRightsAsync(),
                await CheckVirtualizationAsync(),
                await CheckDiskSpaceAsync(targetDrive, requiredSpaceBytes),
                await CheckSevenZipAsync()
            };

            var failedChecks = checks.Where(c => !c.IsSuccess).ToList();
            var warningChecks = checks.Where(c => c.CheckType == SystemCheckType.Virtualization && !c.IsSuccess).ToList();

            if (failedChecks.Any(c => c.CheckType != SystemCheckType.Virtualization))
            {
                var criticalFailures = failedChecks.Where(c => c.CheckType != SystemCheckType.Virtualization).ToList();
                return new SystemCheckResult
                {
                    IsSuccess = false,
                    Message = $"System check failed: {criticalFailures.Count} critical issue(s) found.",
                    Details = string.Join("\n", criticalFailures.Select(c => $"• {c.Message}")),
                    CheckType = SystemCheckType.Overall
                };
            }
            else if (warningChecks.Any())
            {
                return new SystemCheckResult
                {
                    IsSuccess = true,
                    Message = "System check passed with warnings.",
                    Details = string.Join("\n", warningChecks.Select(c => $"⚠ {c.Message}: {c.Details}")),
                    CheckType = SystemCheckType.Overall
                };
            }
            else
            {
                return new SystemCheckResult
                {
                    IsSuccess = true,
                    Message = "All system checks passed successfully.",
                    CheckType = SystemCheckType.Overall
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running system checks");
            return new SystemCheckResult
            {
                IsSuccess = false,
                Message = "Failed to run system checks",
                Details = ex.Message,
                CheckType = SystemCheckType.Overall
            };
        }
    }

    private async Task<string?> Find7ZipExecutableAsync()
    {
        // Check if 7z.exe is in PATH
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "7z.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                return output.Trim().Split('\n').FirstOrDefault()?.Trim();
            }
        }
        catch
        {
            // Fall through to check standard locations
        }

        // Check standard installation paths
        var possiblePaths = new[]
        {
            @"C:\Program Files\7-Zip\7z.exe",
            @"C:\Program Files (x86)\7-Zip\7z.exe"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
} 