namespace CHIMInstaller.Core.Models;

public class SystemCheckResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public SystemCheckType CheckType { get; set; }
}

public enum SystemCheckType
{
    Virtualization,
    DiskSpace,
    AdminRights,
    SevenZip,
    Overall
} 