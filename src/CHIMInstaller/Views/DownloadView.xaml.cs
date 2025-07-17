using CHIMInstaller.ViewModels;
using System.Windows.Controls;

namespace CHIMInstaller.Views;

public partial class DownloadView : UserControl
{
    public DownloadView(DownloadViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
} 