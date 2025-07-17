namespace CHIMInstaller.Services;

public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified view
    /// </summary>
    /// <typeparam name="T">Type of view to navigate to</typeparam>
    void NavigateTo<T>() where T : class;

    /// <summary>
    /// Navigates to the specified view with parameter
    /// </summary>
    /// <typeparam name="T">Type of view to navigate to</typeparam>
    /// <param name="parameter">Parameter to pass to the view</param>
    void NavigateTo<T>(object parameter) where T : class;

    /// <summary>
    /// Navigates back to the previous view
    /// </summary>
    void NavigateBack();

    /// <summary>
    /// Checks if navigation back is possible
    /// </summary>
    bool CanNavigateBack { get; }

    /// <summary>
    /// Event raised when navigation occurs
    /// </summary>
    event EventHandler<NavigationEventArgs>? NavigationOccurred;

    /// <summary>
    /// Gets the current view type
    /// </summary>
    Type? CurrentViewType { get; }
}

public class NavigationEventArgs : EventArgs
{
    public Type ViewType { get; set; } = null!;
    public object? Parameter { get; set; }
} 