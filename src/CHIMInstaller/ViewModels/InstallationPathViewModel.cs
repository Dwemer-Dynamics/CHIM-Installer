using CHIMInstaller.Services;
using CHIMInstaller.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.IO;

namespace CHIMInstaller.ViewModels;

public partial class InstallationPathViewModel : ObservableObject
{
    private readonly ILogger<InstallationPathViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IConfiguration _configuration;

    [ObservableProperty]
    private ObservableCollection<DriveOption> availableDrives = new();

    [ObservableProperty]
    private DriveOption? selectedDrive;

    [ObservableProperty]
    private string customPath = string.Empty;

    [ObservableProperty]
    private bool useCustomPath = false;

    [ObservableProperty]
    private string installationPath = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Select installation drive";

    [ObservableProperty]
    private bool isValidPath = false;

    [ObservableProperty]
    private bool isCreatingDirectory = false;

    public InstallationPathViewModel(
        ILogger<InstallationPathViewModel> logger,
        INavigationService navigationService,
        IConfiguration configuration)
    {
        _logger = logger;
        _navigationService = navigationService;
        _configuration = configuration;

        InitializeDrives();
    }

    [RelayCommand]
    private void SelectDrive(DriveOption drive)
    {
        if (drive != null)
        {
            SelectedDrive = drive;
            UseCustomPath = false;
            UpdateInstallationPath();
        }
    }

    [RelayCommand]
    private void ToggleCustomPath()
    {
        UseCustomPath = !UseCustomPath;
        if (UseCustomPath)
        {
            SelectedDrive = null;
        }
        UpdateInstallationPath();
    }

    [RelayCommand]
    private async Task CreateDirectoryAsync()
    {
        try
        {
            IsCreatingDirectory = true;
            StatusMessage = "Creating installation directory...";

            var targetPath = UseCustomPath ? CustomPath : SelectedDrive?.InstallPath ?? string.Empty;

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                StatusMessage = "Please select a valid installation path.";
                return;
            }

            // Validate the drive/path exists
            var driveLetter = Path.GetPathRoot(targetPath);
            if (!Directory.Exists(driveLetter))
            {
                StatusMessage = $"Drive {driveLetter} does not exist or is not accessible.";
                return;
            }

            // Create CHIM directory
            var chimDirectory = Path.Combine(targetPath, "CHIM");
            
            _logger.LogInformation($"Creating CHIM directory at: {chimDirectory}");

            if (!Directory.Exists(chimDirectory))
            {
                Directory.CreateDirectory(chimDirectory);
                StatusMessage = $"Successfully created directory: {chimDirectory}";
                _logger.LogInformation($"Created CHIM directory: {chimDirectory}");
            }
            else
            {
                StatusMessage = $"Directory already exists: {chimDirectory}";
                _logger.LogInformation($"CHIM directory already exists: {chimDirectory}");
            }

            // Store the path for later use
            InstallationPath = chimDirectory;
            
            // Update configuration or pass to next screen
            _configuration["Installer:InstallationPath"] = chimDirectory;

            // Wait a moment to show success message
            await Task.Delay(1000);

            // Navigate to download screen
            _logger.LogInformation("Installation path set, navigating to download screen");
            _navigationService.NavigateTo<DownloadView>();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied creating directory");
            StatusMessage = "Access denied. Please run as administrator or choose a different location.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating installation directory");
            StatusMessage = $"Error creating directory: {ex.Message}";
        }
        finally
        {
            IsCreatingDirectory = false;
        }
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        _logger.LogInformation("Refreshing available drives");
        InitializeDrives();
    }

    partial void OnCustomPathChanged(string value)
    {
        UpdateInstallationPath();
    }

    partial void OnSelectedDriveChanged(DriveOption? value)
    {
        UpdateInstallationPath();
    }

    private void InitializeDrives()
    {
        try
        {
            AvailableDrives.Clear();

            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .OrderBy(d => d.Name)
                .ToList();

            foreach (var drive in drives)
            {
                var driveOption = new DriveOption
                {
                    DriveLetter = drive.Name.Substring(0, 1),
                    Name = drive.Name,
                    Label = !string.IsNullOrEmpty(drive.VolumeLabel) ? drive.VolumeLabel : "Local Disk",
                    TotalSpace = drive.TotalSize,
                    FreeSpace = drive.AvailableFreeSpace,
                    InstallPath = drive.Name // e.g., "C:\"
                };

                AvailableDrives.Add(driveOption);
            }

            // Auto-select C drive if available
            var defaultDrive = AvailableDrives.FirstOrDefault(d => d.DriveLetter == "C") 
                               ?? AvailableDrives.FirstOrDefault();
            
            if (defaultDrive != null)
            {
                SelectedDrive = defaultDrive;
                UpdateInstallationPath();
            }

            _logger.LogInformation($"Found {AvailableDrives.Count} available drives");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing drives");
            StatusMessage = "Error loading drives. Please try refreshing.";
        }
    }

    private void UpdateInstallationPath()
    {
        try
        {
            string path;
            
            if (UseCustomPath)
            {
                path = CustomPath;
            }
            else if (SelectedDrive != null)
            {
                path = Path.Combine(SelectedDrive.InstallPath, "CHIM");
            }
            else
            {
                path = string.Empty;
            }

            InstallationPath = path;

            // Validate path
            if (string.IsNullOrWhiteSpace(path))
            {
                IsValidPath = false;
                StatusMessage = "Please select an installation location.";
                return;
            }

            // Check if drive exists and has enough space
            var driveLetter = Path.GetPathRoot(path);
            var drive = AvailableDrives.FirstOrDefault(d => d.Name.StartsWith(driveLetter, StringComparison.OrdinalIgnoreCase));
            
            if (drive == null && !UseCustomPath)
            {
                IsValidPath = false;
                StatusMessage = "Selected drive is not available.";
                return;
            }

            // Check space requirements
            var requiredSpaceGB = _configuration.GetValue<double>("Installer:RequiredDiskSpaceGB", 15.0);
            var requiredBytes = (long)(requiredSpaceGB * 1024 * 1024 * 1024);

            if (drive != null && drive.FreeSpace < requiredBytes)
            {
                IsValidPath = false;
                StatusMessage = $"Insufficient disk space. Required: {requiredSpaceGB:F1} GB, Available: {drive.FreeSpace / (1024.0 * 1024.0 * 1024.0):F1} GB";
                return;
            }

            IsValidPath = true;
            StatusMessage = UseCustomPath 
                ? $"Custom path: {path}"
                : $"Install to: {path} ({drive?.FreeSpaceFormatted} available)";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating installation path");
            IsValidPath = false;
            StatusMessage = "Error validating path.";
        }
    }
}

public partial class DriveOption : ObservableObject
{
    [ObservableProperty]
    private string driveLetter = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string label = string.Empty;

    [ObservableProperty]
    private long totalSpace;

    [ObservableProperty]
    private long freeSpace;

    [ObservableProperty]
    private string installPath = string.Empty;

    public string FreeSpaceFormatted => FormatBytes(FreeSpace);
    public string TotalSpaceFormatted => FormatBytes(TotalSpace);
    public double UsagePercentage => TotalSpace > 0 ? ((double)(TotalSpace - FreeSpace) / TotalSpace) * 100.0 : 0.0;

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