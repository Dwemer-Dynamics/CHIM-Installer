using CHIMInstaller.Models;

namespace CHIMInstaller.Services;

public interface IDownloadService
{
    /// <summary>
    /// Downloads a file from the specified URL with progress reporting
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="destinationPath">Local path to save the file</param>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download result</returns>
    Task<DownloadResult> DownloadFileAsync(string url, string destinationPath, 
        IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the integrity of a downloaded file
    /// </summary>
    /// <param name="filePath">Path to the file to verify</param>
    /// <param name="expectedHash">Expected hash value (optional)</param>
    /// <returns>True if file is valid</returns>
    Task<bool> VerifyFileIntegrityAsync(string filePath, string? expectedHash = null);

    /// <summary>
    /// Checks if a file already exists and prompts for action
    /// </summary>
    /// <param name="filePath">Path to check</param>
    /// <returns>Action to take (overwrite, skip, etc.)</returns>
    Task<FileExistsAction> CheckFileExistsAsync(string filePath);

    /// <summary>
    /// Gets the total size of a file from a URL without downloading
    /// </summary>
    /// <param name="url">URL to check</param>
    /// <returns>File size in bytes, or -1 if unknown</returns>
    Task<long> GetFileSizeAsync(string url);

    /// <summary>
    /// Auto-detects downloaded files in common download directories
    /// </summary>
    /// <param name="fileName">Name or pattern of file to find</param>
    /// <returns>List of found file paths</returns>
    Task<List<string>> AutoDetectDownloadedFilesAsync(string fileName);
} 