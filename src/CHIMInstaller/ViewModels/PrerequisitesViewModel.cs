using CHIMInstaller.Core.Models;
using CHIMInstaller.Services;
using CHIMInstaller.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using CoreInterfaces = CHIMInstaller.Core.Interfaces;

namespace CHIMInstaller.ViewModels;

public partial class PrerequisitesViewModel : ObservableObject
{
    private readonly ILogger<PrerequisitesViewModel> _logger;
    private readonly CoreInterfaces.IPrerequisiteService _prerequisiteService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool isInstalling = false;

    [ObservableProperty]
    private bool installationCompleted = false;

    [ObservableProperty]
    private bool restartRequired = false;

    [ObservableProperty]
    private string statusMessage = "Ready to install prerequisites";

    [ObservableProperty]
    private ObservableCollection<PrerequisiteItem> prerequisiteItems = new();

    [ObservableProperty]
    private double overallProgress = 0.0;

    public PrerequisitesViewModel(
        ILogger<PrerequisitesViewModel> logger,
        CoreInterfaces.IPrerequisiteService prerequisiteService,
        INavigationService navigationService)
    {
        _logger = logger;
        _prerequisiteService = prerequisiteService;
        _navigationService = navigationService;
        
        InitializePrerequisiteItems();
    }

    [RelayCommand]
    private async Task StartInstallationAsync()
    {
        try
        {
            IsInstalling = true;
            InstallationCompleted = false;
            RestartRequired = false;
            StatusMessage = "Installing prerequisites...";
            OverallProgress = 0.0;

            _logger.LogInformation("Starting prerequisite installation");

            // Reset all items to pending
            foreach (var item in PrerequisiteItems)
            {
                item.Status = PrerequisiteStatus.Pending;
                item.Message = "Waiting...";
            }

            var totalSteps = PrerequisiteItems.Count;
            var completedSteps = 0;

            // Install Visual C++ Redistributable
            await InstallPrerequisite("Visual C++ Redistributable", async (progress) =>
            {
                var result = await _prerequisiteService.CheckAndInstallVisualCppAsync(progress);
                return result;
            });
            
            completedSteps++;
            OverallProgress = (double)completedSteps / totalSteps * 100.0;

            // Enable Windows Features
            await InstallPrerequisite("Windows Features", async (progress) =>
            {
                var result = await _prerequisiteService.EnableAllRequiredFeaturesAsync(progress);
                if (result.RequiresRestart)
                    RestartRequired = true;
                return result;
            });

            completedSteps++;
            OverallProgress = 100.0;

            // Check overall results
            var failedItems = PrerequisiteItems.Where(p => p.Status == PrerequisiteStatus.Failed).ToList();
            
            if (failedItems.Any())
            {
                StatusMessage = $"Installation completed with {failedItems.Count} error(s).";
                _logger.LogWarning($"Prerequisites installation completed with {failedItems.Count} failures");
            }
            else
            {
                StatusMessage = RestartRequired 
                    ? "Prerequisites installed successfully! A system restart is required." 
                    : "Prerequisites installed successfully!";
                _logger.LogInformation("Prerequisites installation completed successfully");
            }

            InstallationCompleted = true;
            IsInstalling = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during prerequisite installation");
            StatusMessage = "Error during installation. Please try again.";
            IsInstalling = false;
        }
    }

    [RelayCommand]
    private async Task RetryInstallationAsync()
    {
        _logger.LogInformation("Retrying prerequisite installation");
        await StartInstallationAsync();
    }

    [RelayCommand]
    private void Continue()
    {
        if (InstallationCompleted && !RestartRequired)
        {
            _logger.LogInformation("Prerequisites completed, navigating to installation path selection");
            _navigationService.NavigateTo<InstallationPathView>();
        }
    }

    [RelayCommand]
    private void RestartNow()
    {
        if (RestartRequired)
        {
            _logger.LogInformation("User requested system restart");
            StatusMessage = "Initiating system restart...";
            
            // In a real implementation, you might want to save state before restart
            // For now, we'll just show a message
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/r /t 10 /c \"CHIM AI installation requires a system restart to complete.\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }

    [RelayCommand]
    private void RestartLater()
    {
        if (RestartRequired)
        {
            _logger.LogInformation("User chose to restart later, continuing with installation");
            _navigationService.NavigateTo<InstallationPathView>();
        }
    }

    private void InitializePrerequisiteItems()
    {
        PrerequisiteItems.Clear();
        PrerequisiteItems.Add(new PrerequisiteItem
        {
            Name = "Visual C++ Redistributable",
            Description = "Microsoft Visual C++ Redistributable 2015-2022 x64",
            Status = PrerequisiteStatus.Pending,
            Message = "Ready to install"
        });
        
        PrerequisiteItems.Add(new PrerequisiteItem
        {
            Name = "Windows Features",
            Description = "Virtual Machine Platform, Hyper-V Platform, Windows Subsystem for Linux",
            Status = PrerequisiteStatus.Pending,
            Message = "Ready to install"
        });
    }

    private async Task InstallPrerequisite(string itemName, Func<IProgress<string>, Task<InstallationResult>> installAction)
    {
        var item = PrerequisiteItems.First(p => p.Name == itemName);
        
        try
        {
            item.Status = PrerequisiteStatus.Installing;
            item.Message = "Installing...";

            var progress = new Progress<string>(message =>
            {
                item.Message = message;
                StatusMessage = $"Installing {itemName}: {message}";
            });

            var result = await installAction(progress);
            
            if (result.IsSuccess)
            {
                item.Status = PrerequisiteStatus.Completed;
                item.Message = result.Message;
            }
            else
            {
                item.Status = PrerequisiteStatus.Failed;
                item.Message = result.Message;
                item.ErrorDetails = result.ErrorDetails;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error installing prerequisite '{itemName}'");
            item.Status = PrerequisiteStatus.Failed;
            item.Message = "Installation failed due to an error";
            item.ErrorDetails = ex.Message;
        }
    }
}

public partial class PrerequisiteItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private string? errorDetails;

    [ObservableProperty]
    private PrerequisiteStatus status = PrerequisiteStatus.Pending;
}

public enum PrerequisiteStatus
{
    Pending,
    Installing,
    Completed,
    Failed
} 