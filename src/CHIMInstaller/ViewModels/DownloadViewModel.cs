using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CHIMInstaller.ViewModels;

public partial class DownloadViewModel : ObservableObject
{
    private readonly ILogger<DownloadViewModel> _logger;

    public DownloadViewModel(ILogger<DownloadViewModel> logger)
    {
        _logger = logger;
    }
} 