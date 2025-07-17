using CHIMInstaller.Core.Models;
using CHIMInstaller.Services;
using CHIMInstaller.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using CoreInterfaces = CHIMInstaller.Core.Interfaces;

namespace CHIMInstaller.ViewModels;

public partial class SystemCheckViewModel : ObservableObject
{
    private readonly ILogger<SystemCheckViewModel> _logger;
    private readonly CoreInterfaces.ISystemCheckService _systemCheckService;
    private readonly INavigationService _navigationService;
    private readonly IConfiguration _configuration;

    [ObservableProperty]
    private bool isRunningChecks = false;

    [ObservableProperty]
    private bool checksCompleted = false;

    [ObservableProperty]
    private bool allChecksPassed = false;

    [ObservableProperty]
    private string statusMessage = "Preparing system checks...";

    [ObservableProperty]
    private ObservableCollection<SystemCheckItem> checkItems = new();

    public SystemCheckViewModel(
        ILogger<SystemCheckViewModel> logger, 
        CoreInterfaces.ISystemCheckService systemCheckService,
        INavigationService navigationService,
        IConfiguration configuration)
    {
        _logger = logger;
        _systemCheckService = systemCheckService;
        _navigationService = navigationService;
        _configuration = configuration;
        
        _logger.LogInformation("SystemCheckViewModel initialized");
        
        // Initialize check items
        InitializeCheckItems();
        
        // Start checks automatically
        _ = Task.Run(async () => await RunSystemChecksAsync());
    }

    [RelayCommand]
    private async Task RunSystemChecksAsync()
    {
        try
        {
            IsRunningChecks = true;
            ChecksCompleted = false;
            StatusMessage = "Running system checks...";
            
            _logger.LogInformation("Starting system checks");

            // Get target drive and required space from configuration
            var targetDrive = _configuration["Installer:DefaultInstallPath"]?.Substring(0, 1) ?? "C";
            var requiredSpaceGB = _configuration.GetValue<double>("Installer:RequiredDiskSpaceGB", 15.0);
            var requiredSpaceBytes = (long)(requiredSpaceGB * 1024 * 1024 * 1024);

            // Run individual checks
            await RunIndividualCheck("Administrator Rights", 
                () => _systemCheckService.CheckAdminRightsAsync());
            
            await RunIndividualCheck("Disk Space", 
                () => _systemCheckService.CheckDiskSpaceAsync(targetDrive, requiredSpaceBytes));
            
            await RunIndividualCheck("7-Zip Installation", 
                () => _systemCheckService.CheckSevenZipAsync());
            
            await RunIndividualCheck("Virtualization", 
                () => _systemCheckService.CheckVirtualizationAsync());

            // Determine overall result
            var passedChecks = CheckItems.Count(c => c.Status == CheckStatus.Passed);
            var failedChecks = CheckItems.Count(c => c.Status == CheckStatus.Failed);
            var warningChecks = CheckItems.Count(c => c.Status == CheckStatus.Warning);

            if (failedChecks > 0)
            {
                AllChecksPassed = false;
                StatusMessage = $"System check failed: {failedChecks} critical issue(s) found.";
            }
            else if (warningChecks > 0)
            {
                AllChecksPassed = true;
                StatusMessage = $"System check passed with {warningChecks} warning(s).";
            }
            else
            {
                AllChecksPassed = true;
                StatusMessage = "All system checks passed successfully!";
            }

            ChecksCompleted = true;
            IsRunningChecks = false;
            
            _logger.LogInformation($"System checks completed. Passed: {passedChecks}, Failed: {failedChecks}, Warnings: {warningChecks}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running system checks");
            StatusMessage = "Error running system checks. Please try again.";
            IsRunningChecks = false;
        }
    }

    [RelayCommand]
    private async Task RetryChecksAsync()
    {
        _logger.LogInformation("Retrying system checks");
        await RunSystemChecksAsync();
    }

    [RelayCommand]
    private void Continue()
    {
        if (AllChecksPassed)
        {
            _logger.LogInformation("System checks passed, navigating to prerequisites");
            _navigationService.NavigateTo<PrerequisitesView>();
        }
    }

    private void InitializeCheckItems()
    {
        CheckItems.Clear();
        CheckItems.Add(new SystemCheckItem("Administrator Rights", "Checking if running as administrator..."));
        CheckItems.Add(new SystemCheckItem("Disk Space", "Checking available disk space..."));
        CheckItems.Add(new SystemCheckItem("7-Zip Installation", "Checking for 7-Zip availability..."));
        CheckItems.Add(new SystemCheckItem("Virtualization", "Checking BIOS virtualization settings..."));
    }

    private async Task RunIndividualCheck(string checkName, Func<Task<SystemCheckResult>> checkFunction)
    {
        var checkItem = CheckItems.First(c => c.Name == checkName);
        
        try
        {
            checkItem.Status = CheckStatus.Running;
            checkItem.Message = "Running...";
            
            var result = await checkFunction();
            
            if (result.IsSuccess)
            {
                if (result.CheckType == SystemCheckType.Virtualization && result.Message.Contains("NOT enabled"))
                {
                    checkItem.Status = CheckStatus.Warning;
                    checkItem.Message = result.Message;
                    checkItem.Details = result.Details;
                }
                else
                {
                    checkItem.Status = CheckStatus.Passed;
                    checkItem.Message = result.Message;
                }
            }
            else
            {
                checkItem.Status = CheckStatus.Failed;
                checkItem.Message = result.Message;
                checkItem.Details = result.Details;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error running {checkName} check");
            checkItem.Status = CheckStatus.Failed;
            checkItem.Message = "Check failed due to an error";
            checkItem.Details = ex.Message;
        }
        
        // Small delay to show progress
        await Task.Delay(500);
    }
}

public partial class SystemCheckItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private string? details;

    [ObservableProperty]
    private CheckStatus status = CheckStatus.Pending;

    public SystemCheckItem(string name, string initialMessage)
    {
        Name = name;
        Message = initialMessage;
    }
}

public enum CheckStatus
{
    Pending,
    Running,
    Passed,
    Failed,
    Warning
} 