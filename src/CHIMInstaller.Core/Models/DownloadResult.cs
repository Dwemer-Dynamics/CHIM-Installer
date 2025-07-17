namespace CHIMInstaller.Core.Models;

public class DownloadResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? ErrorDetails { get; set; }
} 