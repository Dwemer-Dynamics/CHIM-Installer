namespace CHIMInstaller.Core.Models;

public class DownloadProgress
{
    public long BytesReceived { get; set; }
    public long TotalBytes { get; set; }
    public double PercentComplete => TotalBytes > 0 ? (double)BytesReceived / TotalBytes * 100.0 : 0.0;
    public string? CurrentFileName { get; set; }
    public string? StatusMessage { get; set; }
} 