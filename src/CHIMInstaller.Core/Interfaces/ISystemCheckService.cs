using CHIMInstaller.Core.Models;

namespace CHIMInstaller.Core.Interfaces;

public interface ISystemCheckService
{
    Task<SystemCheckResult> CheckVirtualizationAsync();
    Task<SystemCheckResult> CheckDiskSpaceAsync(string targetDrive, long requiredSpaceBytes);
    Task<SystemCheckResult> CheckAdminRightsAsync();
    Task<SystemCheckResult> CheckSevenZipAsync();
    Task<SystemCheckResult> RunAllChecksAsync(string targetDrive, long requiredSpaceBytes);
} 