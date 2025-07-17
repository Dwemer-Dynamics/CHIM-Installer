using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CHIMInstaller.ViewModels;

public partial class PrerequisitesViewModel : ObservableObject
{
    private readonly ILogger<PrerequisitesViewModel> _logger;

    public PrerequisitesViewModel(ILogger<PrerequisitesViewModel> logger)
    {
        _logger = logger;
    }
} 