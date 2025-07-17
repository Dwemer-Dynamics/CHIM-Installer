using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CHIMInstaller.ViewModels;

public partial class SystemCheckViewModel : ObservableObject
{
    private readonly ILogger<SystemCheckViewModel> _logger;

    public SystemCheckViewModel(ILogger<SystemCheckViewModel> logger)
    {
        _logger = logger;
        _logger.LogInformation("SystemCheckViewModel initialized");
    }
} 