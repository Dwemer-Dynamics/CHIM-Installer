using CHIMInstaller.Core.Models;

namespace CHIMInstaller.Core.Interfaces;

public interface IDownloadService
{
    Task<DownloadResult> DownloadFileAsync(string url, string destinationPath, IProgress<DownloadProgress> progress, CancellationToken cancellationToken = default);
    Task<bool> CheckFileExistsAsync(string filePath);
    string GetExpectedModFileName(string targetDirectory);
    void OpenNexusDownloadPage();
    Task<DownloadResult> EnsureModFileExistsAsync(string chimDirectory, IProgress<DownloadProgress> progress);
} 