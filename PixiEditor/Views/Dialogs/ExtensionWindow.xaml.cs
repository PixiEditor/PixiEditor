using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PixiEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ExtensionWindow.xaml
    /// </summary>
    public partial class ExtensionWindow : Window
    {
        public ExtensionWindow(ExtensionViewModel viewModel)
        {
            DataContext = new ExtensionWindowViewModel(viewModel);
            InitializeComponent();
        }
    }
}
