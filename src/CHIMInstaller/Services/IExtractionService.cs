using CHIMInstaller.Models;

namespace CHIMInstaller.Services;

public interface IExtractionService
{
    /// <summary>
    /// Extracts an archive to the specified destination
    /// </summary>
    /// <param name="archivePath">Path to the archive file</param>
    /// <param name="destinationPath">Destination directory for extraction</param>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extraction result</returns>
    Task<ExtractionResult> ExtractArchiveAsync(string archivePath, string destinationPath,
        IProgress<ExtractionProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of files in an archive without extracting
    /// </summary>
    /// <param name="archivePath">Path to the archive file</param>
    /// <returns>List of file paths in the archive</returns>
    Task<List<string>> GetArchiveContentsAsync(string archivePath);

    /// <summary>
    /// Validates that an archive is not corrupted
    /// </summary>
    /// <param name="archivePath">Path to the archive file</param>
    /// <returns>True if archive is valid</returns>
    Task<bool> ValidateArchiveAsync(string archivePath);

    /// <summary>
    /// Gets the supported archive formats
    /// </summary>
    /// <returns>List of supported file extensions</returns>
    List<string> GetSupportedFormats();

    /// <summary>
    /// Estimates the uncompressed size of an archive
    /// </summary>
    /// <param name="archivePath">Path to the archive file</param>
    /// <returns>Estimated size in bytes</returns>
    Task<long> EstimateUncompressedSizeAsync(string archivePath);
} 