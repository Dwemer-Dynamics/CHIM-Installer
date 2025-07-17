using CHIMInstaller.ViewModels;
using System.Windows.Controls;

namespace CHIMInstaller.Views;

public partial class CompletionView : UserControl
{
    public CompletionView(CompletionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
} 