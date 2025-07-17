namespace CHIMInstaller.Models;

public class DownloadProgress
{
    public long BytesReceived { get; set; }
    public long TotalBytesToReceive { get; set; }
    public double ProgressPercentage => TotalBytesToReceive > 0 ? (double)BytesReceived / TotalBytesToReceive * 100 : 0;
    public TimeSpan ElapsedTime { get; set; }
    public double DownloadSpeedBytesPerSecond { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InstallationProgress
{
    public string CurrentOperation { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; }
    public string? Details { get; set; }
    public bool IsIndeterminate { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan ElapsedTime => DateTime.Now - StartTime;
}

public class ExtractionProgress
{
    public int FilesExtracted { get; set; }
    public int TotalFiles { get; set; }
    public long BytesExtracted { get; set; }
    public long TotalBytes { get; set; }
    public double ProgressPercentage => TotalFiles > 0 ? (double)FilesExtracted / TotalFiles * 100 : 0;
    public string CurrentFile { get; set; } = string.Empty;
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
} 