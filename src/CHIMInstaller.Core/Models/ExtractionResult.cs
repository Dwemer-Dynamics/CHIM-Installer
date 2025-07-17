namespace CHIMInstaller.Core.Models;

public class ExtractionResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ExtractedPath { get; set; }
    public string? ErrorDetails { get; set; }
    public int ExitCode { get; set; }
} 