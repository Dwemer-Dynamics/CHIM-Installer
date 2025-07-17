using CHIMInstaller.Models;
using CHIMInstaller.Services;
using CHIMInstaller.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace CHIMInstaller.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private string version = "1.0.0";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool showStepProgress = true;

    [ObservableProperty]
    private bool isInstallationInProgress;

    [ObservableProperty]
    private ObservableCollection<InstallationStep> installationSteps;

    [ObservableProperty]
    private int currentStep = 1;

    public MainWindowViewModel(
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        ILogger<MainWindowViewModel> logger)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Initialize installation steps
        InstallationSteps = new ObservableCollection<InstallationStep>
        {
            new() { StepNumber = 1, StepName = "Welcome", IsCompleted = false, IsActive = true },
            new() { StepNumber = 2, StepName = "System Check", IsCompleted = false },
            new() { StepNumber = 3, StepName = "Prerequisites", IsCompleted = false },
            new() { StepNumber = 4, StepName = "Download", IsCompleted = false },
            new() { StepNumber = 5, StepName = "Install", IsCompleted = false },
            new() { StepNumber = 6, StepName = "Complete", IsCompleted = false, IsLastStep = true }
        };

        // Subscribe to navigation events
        _navigationService.NavigationOccurred += OnNavigationOccurred;

        // Start with welcome screen
        NavigateToWelcome();
    }

    private void OnNavigationOccurred(object? sender, NavigationEventArgs e)
    {
        _logger.LogInformation("Navigated to {ViewType}", e.ViewType.Name);
        
        // Update current view
        CurrentView = _serviceProvider.GetService(e.ViewType);
        
        // Update step progress based on current view
        UpdateStepProgress(e.ViewType);
    }

    private void UpdateStepProgress(Type viewType)
    {
        var stepIndex = viewType.Name switch
        {
            nameof(WelcomeView) => 0,
            nameof(SystemCheckView) => 1,
            nameof(PrerequisitesView) => 2,
            nameof(DownloadView) => 3,
            nameof(InstallationProgressView) => 4,
            nameof(CompletionView) => 5,
            _ => -1
        };

        if (stepIndex >= 0)
        {
            CurrentStep = stepIndex + 1;
            
            // Mark previous steps as completed
            for (int i = 0; i < stepIndex; i++)
            {
                InstallationSteps[i].IsCompleted = true;
                InstallationSteps[i].IsActive = false;
            }
            
            // Mark current step as active
            InstallationSteps[stepIndex].IsActive = true;
            
            // Mark future steps as not active
            for (int i = stepIndex + 1; i < InstallationSteps.Count; i++)
            {
                InstallationSteps[i].IsActive = false;
            }
        }
    }

    [RelayCommand]
    private void NavigateToWelcome()
    {
        _navigationService.NavigateTo<WelcomeView>();
    }

    [RelayCommand]
    private void NavigateToSystemCheck()
    {
        _navigationService.NavigateTo<SystemCheckView>();
    }

    [RelayCommand]
    private void NavigateToPrerequisites()
    {
        _navigationService.NavigateTo<PrerequisitesView>();
    }

    [RelayCommand]
    private void NavigateToDownload()
    {
        _navigationService.NavigateTo<DownloadView>();
    }

    [RelayCommand]
    private void NavigateToInstallation()
    {
        _navigationService.NavigateTo<InstallationProgressView>();
        IsInstallationInProgress = true;
    }

    [RelayCommand]
    private void NavigateToCompletion()
    {
        _navigationService.NavigateTo<CompletionView>();
        IsInstallationInProgress = false;
        
        // Mark all steps as completed
        foreach (var step in InstallationSteps)
        {
            step.IsCompleted = true;
            step.IsActive = false;
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        if (_navigationService.CanNavigateBack)
        {
            _navigationService.NavigateBack();
        }
    }

    public void CancelInstallation()
    {
        try
        {
            _logger.LogInformation("Canceling installation...");
            
            // TODO: Implement cancellation logic
            // - Stop any downloads
            // - Clean up partial installations
            // - Reset system state if needed
            
            IsInstallationInProgress = false;
            IsLoading = false;
            
            _logger.LogInformation("Installation canceled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during installation cancellation");
            MessageBox.Show($"Error canceling installation: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ShowHelp()
    {
        try
        {
            // Open help documentation or show help dialog
            var helpUrl = "https://github.com/your-repo/CHIM-installer/wiki";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = helpUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening help");
            MessageBox.Show("Unable to open help documentation. Please visit the project repository for assistance.", 
                "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public void OnWindowClosing()
    {
        _logger.LogInformation("Main window closing...");
        
        // Unsubscribe from events
        _navigationService.NavigationOccurred -= OnNavigationOccurred;
        
        // Save any state or configuration if needed
        // TODO: Implement state saving
        
        _logger.LogInformation("Window cleanup completed");
    }

    public void SetLoading(bool isLoading, string? message = null)
    {
        IsLoading = isLoading;
        
        if (!string.IsNullOrEmpty(message))
        {
            _logger.LogInformation("Loading state changed: {IsLoading} - {Message}", isLoading, message);
        }
    }

    public void CompleteStep(int stepNumber)
    {
        if (stepNumber > 0 && stepNumber <= InstallationSteps.Count)
        {
            var step = InstallationSteps[stepNumber - 1];
            step.IsCompleted = true;
            step.IsActive = false;
            
            _logger.LogInformation("Step {StepNumber} completed: {StepName}", stepNumber, step.StepName);
        }
    }

    public void SetActiveStep(int stepNumber)
    {
        if (stepNumber > 0 && stepNumber <= InstallationSteps.Count)
        {
            // Deactivate all steps
            foreach (var step in InstallationSteps)
            {
                step.IsActive = false;
            }
            
            // Activate current step
            var currentStepObj = InstallationSteps[stepNumber - 1];
            currentStepObj.IsActive = true;
            
            CurrentStep = stepNumber;
            
            _logger.LogInformation("Active step set to {StepNumber}: {StepName}", stepNumber, currentStepObj.StepName);
        }
    }
}

public partial class InstallationStep : ObservableObject
{
    [ObservableProperty]
    private int stepNumber;

    [ObservableProperty]
    private string stepName = string.Empty;

    [ObservableProperty]
    private bool isCompleted;

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private bool isLastStep;
} 