using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CHIMInstaller.ViewModels;

public partial class InstallationPathViewModel : ObservableObject
{
    private readonly ILogger<InstallationPathViewModel> _logger;

    public InstallationPathViewModel(ILogger<InstallationPathViewModel> logger)
    {
        _logger = logger;
    }
} 