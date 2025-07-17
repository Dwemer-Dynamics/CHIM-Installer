using CHIMInstaller.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace CHIMInstaller.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<WelcomeViewModel> _logger;

    [ObservableProperty]
    private string versionText = "Version 1.2.0 Beta";

    [ObservableProperty]
    private ObservableCollection<FeatureItem> features;

    [ObservableProperty]
    private ObservableCollection<string> minimumRequirements;

    [ObservableProperty]
    private ObservableCollection<string> recommendedRequirements;

    public WelcomeViewModel(INavigationService navigationService, ILogger<WelcomeViewModel> logger)
    {
        _navigationService = navigationService;
        _logger = logger;

        InitializeFeatures();
        InitializeRequirements();
    }

    private void InitializeFeatures()
    {
        Features = new ObservableCollection<FeatureItem>
        {
            new()
            {
                Icon = "💬",
                Title = "Intelligent Conversations",
                Description = "NPCs engage in natural, context-aware dialogue powered by advanced AI"
            },
            new()
            {
                Icon = "👥",
                Title = "Dynamic Personalities",
                Description = "Each NPC has unique personality traits that evolve based on your interactions"
            },
            new()
            {
                Icon = "🎤",
                Title = "Voice Recognition",
                Description = "Speak naturally to NPCs using advanced speech-to-text technology"
            },
            new()
            {
                Icon = "🧠",
                Title = "Adaptive AI",
                Description = "AI learns and remembers your choices, creating personalized experiences"
            },
            new()
            {
                Icon = "📖",
                Title = "Lore Integration",
                Description = "Deep integration with Elder Scrolls lore and world-building"
            },
            new()
            {
                Icon = "⚙️",
                Title = "Easy Configuration",
                Description = "Simple setup with the included CHIM Launcher and management tools"
            }
        };
    }

    private void InitializeRequirements()
    {
        MinimumRequirements = new ObservableCollection<string>
        {
            "Windows 10 64-bit",
            "8 GB RAM",
            "10 GB free disk space",
            "DirectX 11 compatible GPU",
            "Internet connection",
            "Skyrim Special Edition"
        };

        RecommendedRequirements = new ObservableCollection<string>
        {
            "Windows 11 64-bit",
            "16+ GB RAM",
            "50+ GB free disk space",
            "RTX 3060 / RX 6600 XT or better",
            "High-speed internet",
            "SSD for best performance"
        };
    }

    [RelayCommand]
    private void Continue()
    {
        try
        {
            _logger.LogInformation("User continuing from welcome screen to system check");
            
            // Navigate to system check screen
            _navigationService.NavigateTo<Views.SystemCheckView>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to system check");
            MessageBox.Show($"Error navigating to system check: {ex.Message}", "Navigation Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Exit()
    {
        try
        {
            _logger.LogInformation("User requested to exit from welcome screen");
            
            var result = MessageBox.Show(
                "Are you sure you want to exit the CHIM AI Installer?", 
                "Confirm Exit", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                _logger.LogInformation("User confirmed exit, closing application");
                Application.Current.Shutdown();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during exit confirmation");
            Application.Current.Shutdown();
        }
    }

    [RelayCommand]
    private void ShowMoreInfo()
    {
        try
        {
            // Open project website or documentation
            var url = "https://github.com/your-org/CHIM-installer";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening project information");
            MessageBox.Show("Unable to open project information. Please visit the project repository manually.", 
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ShowSystemRequirements()
    {
        // Could open a detailed system requirements dialog or window
        MessageBox.Show(
            "Detailed system requirements:\n\n" +
            "• Windows 10 64-bit or newer\n" +
            "• Minimum 8 GB RAM (16+ GB recommended)\n" +
            "• 10+ GB free disk space\n" +
            "• DirectX 11 compatible graphics card\n" +
            "• Internet connection for downloads\n" +
            "• Skyrim Special Edition (latest version)\n" +
            "• Administrator privileges\n" +
            "• Virtualization enabled in BIOS (recommended)",
            "System Requirements",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

public class FeatureItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 