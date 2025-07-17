using CHIMInstaller.ViewModels;
using System.Windows.Controls;

namespace CHIMInstaller.Views;

public partial class InstallationProgressView : UserControl
{
    public InstallationProgressView(InstallationProgressViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
} 