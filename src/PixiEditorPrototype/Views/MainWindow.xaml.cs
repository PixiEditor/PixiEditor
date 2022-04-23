using System.Windows;
using System.Windows.Controls;
using PixiEditorPrototype.ViewModels;

namespace PixiEditorPrototype.Views;

internal partial class MainWindow : Window, IMainView
{
    public MainWindow()
    {
        InitializeComponent();
        ((ViewModelMain)DataContext).View = this;
    }

    private Image? GetImage() => (Image?)((Border?)zoombox.AdditionalContent)?.Child;

    public void ForceRefreshFinalImage()
    {
        GetImage()?.InvalidateVisual();
    }
}
