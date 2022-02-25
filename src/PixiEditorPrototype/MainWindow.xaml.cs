using PixiEditorPrototype.ViewModels;
using System.Windows;

namespace PixiEditorPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        public DocumentViewModel Document { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
