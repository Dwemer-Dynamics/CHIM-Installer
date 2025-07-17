using CHIMInstaller.ViewModels;
using System.Windows.Controls;

namespace CHIMInstaller.Views;

public partial class PrerequisitesView : UserControl
{
    public PrerequisitesView(PrerequisitesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
} 