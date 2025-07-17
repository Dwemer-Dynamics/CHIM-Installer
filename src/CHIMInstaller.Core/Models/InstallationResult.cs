namespace CHIMInstaller.Core.Models;

public class InstallationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public bool RequiresRestart { get; set; }
    public int ExitCode { get; set; }
} 