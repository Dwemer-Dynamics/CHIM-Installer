namespace CHIMInstaller.Models;

public class SystemCheckResult
{
    public bool IsAdministrator { get; set; }
    public bool IsVirtualizationEnabled { get; set; }
    public bool IsWindowsVersionSupported { get; set; }
    public bool IsInternetAvailable { get; set; }
    public long AvailableDiskSpace { get; set; }
    public string? DriveLetter { get; set; }
    public Dictionary<string, bool> WindowsFeatures { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public bool OverallResult => Errors.Count == 0;
    public DateTime CheckedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Minimum required disk space in bytes (10 GB)
    /// </summary>
    public const long MinimumDiskSpaceRequired = 10L * 1024 * 1024 * 1024;

    /// <summary>
    /// Required Windows features for CHIM AI
    /// </summary>
    public static readonly List<string> RequiredWindowsFeatures = new()
    {
        "VirtualMachinePlatform",
        "HypervisorPlatform", 
        "Microsoft-Windows-Subsystem-Linux"
    };

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
} 