using CHIMInstaller.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CHIMInstaller.Services;

public class PrerequisiteService : IPrerequisiteService
{
    private readonly ILogger<PrerequisiteService> _logger;

    public PrerequisiteService(ILogger<PrerequisiteService> logger)
    {
        _logger = logger;
    }

    public async Task<InstallationResult> InstallVisualCppRedistributableAsync(IProgress<InstallationProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return new InstallationResult { Success = true };
    }

    public async Task<InstallationResult> Install7ZipAsync(IProgress<InstallationProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return new InstallationResult { Success = true };
    }

    public async Task<InstallationResult> EnableWindowsFeaturesAsync(List<string> features, IProgress<InstallationProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return new InstallationResult { Success = true };
    }

    public bool IsVisualCppRedistributableInstalled()
    {
        return true;
    }

    public bool Is7ZipInstalled()
    {
        return true;
    }

    public async Task<Dictionary<string, bool>> GetWindowsFeaturesStatusAsync()
    {
        return new Dictionary<string, bool>
        {
            ["VirtualMachinePlatform"] = true,
            ["HypervisorPlatform"] = true,
            ["Microsoft-Windows-Subsystem-Linux"] = true
        };
    }
} 