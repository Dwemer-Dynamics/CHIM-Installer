using CHIMInstaller.Core.Interfaces;
using CHIMInstaller.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CHIMInstaller.Core.Services;

public class ExtractionService : IExtractionService
{
    private readonly ILogger<ExtractionService> _logger;

    public ExtractionService(ILogger<ExtractionService> logger)
    {
        _logger = logger;
    }

    public async Task<ExtractionResult> ExtractArchiveAsync(string archivePath, string extractionPath, IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        try
        {
            progress?.Report($"Starting extraction of '{Path.GetFileName(archivePath)}'...");
            _logger.LogInformation($"Extracting archive: {archivePath} to {extractionPath}");

            if (!File.Exists(archivePath))
            {
                return new ExtractionResult
                {
                    IsSuccess = false,
                    Message = "Archive file not found",
                    ErrorDetails = $"The file '{archivePath}' does not exist."
                };
            }

            var sevenZipExecutable = await Find7ZipExecutableAsync();
            
            if (string.IsNullOrEmpty(sevenZipExecutable))
            {
                return new ExtractionResult
                {
                    IsSuccess = false,
                    Message = "7-Zip not found",
                    ErrorDetails = "7-Zip executable (7z.exe) not found or not in system PATH. Please install 7-Zip from https://www.7-zip.org/"
                };
            }

            progress?.Report($"Found 7-Zip at: {sevenZipExecutable}");
            _logger.LogInformation($"Found 7-Zip at: {sevenZipExecutable}");

            // Ensure extraction directory exists
            if (!Directory.Exists(extractionPath))
            {
                Directory.CreateDirectory(extractionPath);
            }

            progress?.Report($"Extracting to '{extractionPath}'...");
            
            // 7-Zip command: x = extract with full paths, -o = output directory, -y = yes to all prompts
            var arguments = $"x \"{archivePath}\" -o\"{extractionPath}\" -y";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipExecutable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            _logger.LogInformation($"Running 7-Zip with arguments: {arguments}");

            var outputData = new List<string>();
            var errorData = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputData.Add(e.Data);
                    
                    // Report progress for interesting lines
                    if (e.Data.Contains("Extracting") || e.Data.Contains("%"))
                    {
                        progress?.Report(e.Data.Trim());
                    }
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorData.Add(e.Data);
                    _logger.LogWarning($"7-Zip stderr: {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            var output = string.Join("\n", outputData);
            var error = string.Join("\n", errorData);

            if (process.ExitCode == 0)
            {
                progress?.Report("Extraction completed successfully.");
                _logger.LogInformation($"Successfully extracted '{archivePath}' to '{extractionPath}'");
                
                return new ExtractionResult
                {
                    IsSuccess = true,
                    Message = "Archive extracted successfully",
                    ExtractedPath = extractionPath,
                    ExitCode = process.ExitCode
                };
            }
            else
            {
                _logger.LogError($"7-Zip extraction failed with exit code {process.ExitCode}. Output: {output}, Error: {error}");
                
                var errorMessage = GetFriendlyErrorMessage(process.ExitCode, error);
                
                return new ExtractionResult
                {
                    IsSuccess = false,
                    Message = errorMessage,
                    ErrorDetails = $"Exit code: {process.ExitCode}\nOutput: {output}\nError: {error}",
                    ExitCode = process.ExitCode
                };
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Extraction was cancelled");
            return new ExtractionResult
            {
                IsSuccess = false,
                Message = "Extraction was cancelled"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during extraction");
            return new ExtractionResult
            {
                IsSuccess = false,
                Message = "Extraction failed due to an unexpected error",
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<bool> Is7ZipAvailableAsync()
    {
        var executable = await Find7ZipExecutableAsync();
        return !string.IsNullOrEmpty(executable);
    }

    public async Task<string> Find7ZipExecutableAsync()
    {
        // Check if 7z.exe is in PATH
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "7z.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var path = output.Trim().Split('\n').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    return path;
                }
            }
        }
        catch
        {
            // Fall through to check standard locations
        }

        // Check standard installation paths
        var possiblePaths = new[]
        {
            @"C:\Program Files\7-Zip\7z.exe",
            @"C:\Program Files (x86)\7-Zip\7z.exe"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return string.Empty;
    }

    private string GetFriendlyErrorMessage(int exitCode, string errorOutput)
    {
        return exitCode switch
        {
            1 => "7-Zip encountered warnings during extraction",
            2 => "7-Zip encountered a fatal error",
            7 => "7-Zip command line error",
            8 => "Not enough memory for operation",
            255 => "User stopped the process",
            _ when errorOutput.Contains("corrupted", StringComparison.OrdinalIgnoreCase) => 
                "The archive appears to be corrupted. Please download the file again.",
            _ when errorOutput.Contains("password", StringComparison.OrdinalIgnoreCase) => 
                "The archive is password protected.",
            _ when errorOutput.Contains("disk", StringComparison.OrdinalIgnoreCase) => 
                "Insufficient disk space for extraction.",
            _ => $"7-Zip failed with exit code {exitCode}. This might indicate a corrupted archive, insufficient disk space, or permission issues."
        };
    }
} 