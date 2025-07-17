using CHIMInstaller.Core.Interfaces;
using CHIMInstaller.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CHIMInstaller.Core.Services;

public class DownloadService : IDownloadService
{
    private readonly ILogger<DownloadService> _logger;
    private readonly HttpClient _httpClient;

    public const string NexusDownloadUrl = "https://www.nexusmods.com/skyrimspecialedition/mods/126330?tab=files&file_id=626114";
    public const string ExpectedFileName = "DwemerAI4Skyrim3-126330-1-2-0-1746980508.7z";

    public DownloadService(ILogger<DownloadService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<DownloadResult> DownloadFileAsync(string url, string destinationPath, IProgress<DownloadProgress> progress, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Starting download from {url} to {destinationPath}");
            
            progress?.Report(new DownloadProgress
            {
                StatusMessage = "Starting download...",
                CurrentFileName = Path.GetFileName(destinationPath)
            });

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new DownloadResult
                {
                    IsSuccess = false,
                    Message = $"Download failed with status: {response.StatusCode}",
                    ErrorDetails = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
                };
            }

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                progress?.Report(new DownloadProgress
                {
                    BytesReceived = totalBytesRead,
                    TotalBytes = totalBytes,
                    CurrentFileName = Path.GetFileName(destinationPath),
                    StatusMessage = $"Downloaded {FormatBytes(totalBytesRead)}" + 
                                  (totalBytes > 0 ? $" of {FormatBytes(totalBytes)}" : "")
                });
            }

            var fileInfo = new FileInfo(destinationPath);
            
            _logger.LogInformation($"Download completed successfully. File size: {fileInfo.Length} bytes");

            return new DownloadResult
            {
                IsSuccess = true,
                Message = "Download completed successfully",
                FilePath = destinationPath,
                FileSize = fileInfo.Length
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Download was cancelled");
            return new DownloadResult
            {
                IsSuccess = false,
                Message = "Download was cancelled"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during download");
            return new DownloadResult
            {
                IsSuccess = false,
                Message = "Download failed",
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<bool> CheckFileExistsAsync(string filePath)
    {
        try
        {
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if file exists: {filePath}");
            return false;
        }
    }

    public string GetExpectedModFileName(string targetDirectory)
    {
        return Path.Combine(targetDirectory, ExpectedFileName);
    }

    /// <summary>
    /// Opens the Nexus download page in the user's browser (fallback when direct download isn't possible)
    /// </summary>
    public void OpenNexusDownloadPage()
    {
        try
        {
            _logger.LogInformation("Opening Nexus download page in browser");
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = NexusDownloadUrl,
                    UseShellExecute = true
                }
            };
            
            process.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Nexus download page");
        }
    }

    /// <summary>
    /// Checks if the mod file exists, and if not, guides the user to download it manually
    /// </summary>
    public async Task<DownloadResult> EnsureModFileExistsAsync(string chimDirectory, IProgress<DownloadProgress> progress)
    {
        try
        {
            var expectedFilePath = GetExpectedModFileName(chimDirectory);
            
            progress?.Report(new DownloadProgress
            {
                StatusMessage = $"Checking for mod file: {ExpectedFileName}"
            });

            _logger.LogInformation($"Checking for mod file at: {expectedFilePath}");

            if (await CheckFileExistsAsync(expectedFilePath))
            {
                var fileInfo = new FileInfo(expectedFilePath);
                return new DownloadResult
                {
                    IsSuccess = true,
                    Message = "Mod file found successfully",
                    FilePath = expectedFilePath,
                    FileSize = fileInfo.Length
                };
            }

            // File not found - need manual download
            _logger.LogWarning($"Mod file not found at: {expectedFilePath}");

            progress?.Report(new DownloadProgress
            {
                StatusMessage = "Mod file not found. Opening download page..."
            });

            return new DownloadResult
            {
                IsSuccess = false,
                Message = "Mod file not found - manual download required",
                ErrorDetails = $"Please download '{ExpectedFileName}' from Nexus Mods and place it in: {chimDirectory}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for mod file");
            return new DownloadResult
            {
                IsSuccess = false,
                Message = "Error checking for mod file",
                ErrorDetails = ex.Message
            };
        }
    }

    private static string FormatBytes(long bytes)
    {
        const int scale = 1024;
        string[] orders = { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return $"{decimal.Divide(bytes, max):##.##} {order}";

            max /= scale;
        }
        return "0 Bytes";
    }
} 