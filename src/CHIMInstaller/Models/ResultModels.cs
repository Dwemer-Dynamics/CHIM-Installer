namespace CHIMInstaller.Models;

public class DownloadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public bool WasResumed { get; set; }
}

public class InstallationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> InstalledComponents { get; set; } = new();
    public List<string> FailedComponents { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public bool RequiresRestart { get; set; }
    public string? LogFilePath { get; set; }
}

public class ExtractionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int FilesExtracted { get; set; }
    public long TotalBytesExtracted { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> ExtractedFiles { get; set; } = new();
    public string? DestinationPath { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}

public enum FileExistsAction
{
    Overwrite,
    Skip,
    Rename,
    Cancel
}

public enum InstallationComponentType
{
    Core,
    Optional,
    Recommended
} 