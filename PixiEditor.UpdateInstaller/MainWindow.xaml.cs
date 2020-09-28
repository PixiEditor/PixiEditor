using System.Reflection;
using System.Windows;

namespace PixiEditor.UpdateInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModelMain(Close);
        }
    }
}
