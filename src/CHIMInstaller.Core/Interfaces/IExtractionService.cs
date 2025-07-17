using CHIMInstaller.Core.Models;

namespace CHIMInstaller.Core.Interfaces;

public interface IExtractionService
{
    Task<ExtractionResult> ExtractArchiveAsync(string archivePath, string extractionPath, IProgress<string> progress, CancellationToken cancellationToken = default);
    Task<bool> Is7ZipAvailableAsync();
    Task<string> Find7ZipExecutableAsync();
} 