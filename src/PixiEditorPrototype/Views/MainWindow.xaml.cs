using PixiEditorPrototype.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditorPrototype.Views
{
    internal partial class MainWindow : Window, IMainView
    {
        public MainWindow()
        {
            InitializeComponent();
            ((ViewModelMain)DataContext).View = this;
        }
        public void ForceRefreshFinalImage()
        {
            ((Image?)((Border?)zoombox.AdditionalContent)?.Child)?.InvalidateVisual();
        }
    }
}
