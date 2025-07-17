using CHIMInstaller.ViewModels;
using System.Windows.Controls;

namespace CHIMInstaller.Views;

/// <summary>
/// Interaction logic for SystemCheckView.xaml
/// </summary>
public partial class SystemCheckView : UserControl
{
    public SystemCheckView(SystemCheckViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
} 