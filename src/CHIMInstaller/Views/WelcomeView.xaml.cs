using CHIMInstaller.ViewModels;
using System.Windows.Controls;

namespace CHIMInstaller.Views;

/// <summary>
/// Interaction logic for WelcomeView.xaml
/// </summary>
public partial class WelcomeView : UserControl
{
    public WelcomeView(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
} 