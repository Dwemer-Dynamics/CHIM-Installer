using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CHIMInstaller.ViewModels;

public partial class CompletionViewModel : ObservableObject
{
    private readonly ILogger<CompletionViewModel> _logger;

    public CompletionViewModel(ILogger<CompletionViewModel> logger)
    {
        _logger = logger;
    }
} 