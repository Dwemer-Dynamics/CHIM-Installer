using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CHIMInstaller.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigationService> _logger;
    private readonly Stack<Type> _navigationHistory = new();

    public Type? CurrentViewType { get; private set; }
    public bool CanNavigateBack => _navigationHistory.Count > 1;

    public event EventHandler<NavigationEventArgs>? NavigationOccurred;

    public NavigationService(IServiceProvider serviceProvider, ILogger<NavigationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void NavigateTo<T>() where T : class
    {
        NavigateToInternal(typeof(T), null);
    }

    public void NavigateTo<T>(object parameter) where T : class
    {
        NavigateToInternal(typeof(T), parameter);
    }

    private void NavigateToInternal(Type viewType, object? parameter)
    {
        try
        {
            _logger.LogInformation("Navigating to {ViewType}", viewType.Name);

            // Verify the view is registered in DI
            var view = _serviceProvider.GetService(viewType);
            if (view == null)
            {
                _logger.LogError("View {ViewType} is not registered in the service container", viewType.Name);
                throw new InvalidOperationException($"View {viewType.Name} is not registered in the service container");
            }

            // Add to navigation history
            if (CurrentViewType != null)
            {
                _navigationHistory.Push(CurrentViewType);
            }

            // Update current view
            CurrentViewType = viewType;

            // Raise navigation event
            var args = new NavigationEventArgs
            {
                ViewType = viewType,
                Parameter = parameter
            };

            NavigationOccurred?.Invoke(this, args);

            _logger.LogInformation("Successfully navigated to {ViewType}", viewType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to {ViewType}", viewType.Name);
            throw;
        }
    }

    public void NavigateBack()
    {
        if (!CanNavigateBack)
        {
            _logger.LogWarning("Cannot navigate back - no previous view in history");
            return;
        }

        try
        {
            // Get previous view from history
            var previousViewType = _navigationHistory.Pop();
            
            _logger.LogInformation("Navigating back to {ViewType}", previousViewType.Name);

            // Update current view (don't add to history since we're going back)
            CurrentViewType = previousViewType;

            // Raise navigation event
            var args = new NavigationEventArgs
            {
                ViewType = previousViewType,
                Parameter = null
            };

            NavigationOccurred?.Invoke(this, args);

            _logger.LogInformation("Successfully navigated back to {ViewType}", previousViewType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back");
            throw;
        }
    }

    public void ClearHistory()
    {
        _navigationHistory.Clear();
        _logger.LogInformation("Navigation history cleared");
    }

    public IReadOnlyList<Type> GetNavigationHistory()
    {
        return _navigationHistory.ToList().AsReadOnly();
    }

    public void Reset()
    {
        _navigationHistory.Clear();
        CurrentViewType = null;
        _logger.LogInformation("Navigation service reset");
    }
} 