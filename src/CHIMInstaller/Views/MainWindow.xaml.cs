using CHIMInstaller.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CHIMInstaller.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Center the window on the screen
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Handle window state changes
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        // Handle minimize/restore behavior if needed
        if (WindowState == WindowState.Minimized)
        {
            // Could implement system tray behavior here
        }
    }

    private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Allow dragging the window by clicking on the title bar
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Handle close button click
        var result = MessageBox.Show(
            "Are you sure you want to close the CHIM AI Installer?", 
            "Confirm Exit", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            // Check if installation is in progress
            if (DataContext is MainWindowViewModel viewModel && viewModel.IsInstallationInProgress)
            {
                var cancelResult = MessageBox.Show(
                    "Installation is currently in progress. Closing now may leave your system in an incomplete state.\n\nAre you sure you want to cancel the installation?",
                    "Installation In Progress",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                    
                if (cancelResult == MessageBoxResult.Yes)
                {
                    // Cancel any ongoing operations
                    viewModel.CancelInstallation();
                    Close();
                }
            }
            else
            {
                Close();
            }
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        // Minimize the window
        WindowState = WindowState.Minimized;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Handle any cleanup before closing
        if (DataContext is MainWindowViewModel viewModel)
        {
            // Save any configuration or state if needed
            viewModel.OnWindowClosing();
        }
        
        base.OnClosing(e);
    }

    // Handle keyboard shortcuts
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                // ESC key to close (with confirmation)
                CloseButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
                
            case Key.F1:
                // F1 for help
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.ShowHelp();
                }
                e.Handled = true;
                break;
                
            case Key.F11:
                // F11 for full screen toggle (if desired)
                ToggleFullScreen();
                e.Handled = true;
                break;
        }
        
        base.OnKeyDown(e);
    }

    private void ToggleFullScreen()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
        }
        else
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }
    }

    // Handle window size restrictions
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        // Set minimum window size
        MinWidth = 800;
        MinHeight = 500;
        
        // Set maximum window size (optional)
        MaxWidth = 1200;
        MaxHeight = 800;
    }
} 