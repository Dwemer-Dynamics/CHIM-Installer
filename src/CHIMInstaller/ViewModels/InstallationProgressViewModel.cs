using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CHIMInstaller.ViewModels;

public partial class InstallationProgressViewModel : ObservableObject
{
    private readonly ILogger<InstallationProgressViewModel> _logger;

    public InstallationProgressViewModel(ILogger<InstallationProgressViewModel> logger)
    {
        _logger = logger;
    }
} 