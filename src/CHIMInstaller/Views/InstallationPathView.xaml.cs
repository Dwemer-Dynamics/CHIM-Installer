using CHIMInstaller.ViewModels;
using System.Windows.Controls;

namespace CHIMInstaller.Views;

public partial class InstallationPathView : UserControl
{
    public InstallationPathView(InstallationPathViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
} 