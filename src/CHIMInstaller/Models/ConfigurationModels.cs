using System.Collections.Generic;

namespace CHIMInstaller.Models;

public class InstallationConfiguration
{
    public string InstallationPath { get; set; } = string.Empty;
    public List<string> SelectedComponents { get; set; } = new();
    public bool CreateDesktopShortcut { get; set; } = true;
    public bool CreateStartMenuShortcut { get; set; } = true;
    public bool RunAfterInstallation { get; set; } = false;
    public string? NexusModsFilePath { get; set; }
    public Dictionary<string, object> AdvancedSettings { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Version { get; set; } = "1.0.0";
}

public class InstallationComponent
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InstallationComponentType Type { get; set; }
    public long SizeBytes { get; set; }
    public bool IsSelected { get; set; }
    public bool IsRequired { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public string? InstallationPath { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class InstallerSettings
{
    public string DefaultInstallationPath { get; set; } = @"C:\CHIM";
    public string NexusModsUrl { get; set; } = "https://www.nexusmods.com/skyrimspecialedition/mods/126330?tab=files&file_id=626114";
    public string ExpectedFileName { get; set; } = "DwemerAI4Skyrim3-126330-1-2-0-1746980508.7z";
    public string VisualCppDownloadUrl { get; set; } = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
    public string SevenZipDownloadUrl { get; set; } = "https://www.7-zip.org/a/7z2301-x64.exe";
    public List<string> RequiredWindowsFeatures { get; set; } = SystemCheckResult.RequiredWindowsFeatures;
    public long MinimumDiskSpace { get; set; } = SystemCheckResult.MinimumDiskSpaceRequired;
    public int MaxDownloadRetries { get; set; } = 3;
    public int DownloadTimeoutSeconds { get; set; } = 300;
}

public static class ComponentNames
{
    public const string Core = "Core";
    public const string HerikaServer = "Herika Server";
    public const string AIAgent = "AI Agent";
    public const string WebSocketSTT = "WebSocket STT";
    public const string CHIMLauncher = "CHIM Launcher";
    public const string Documentation = "Documentation";
    public const string SampleConfigs = "Sample Configurations";
} 