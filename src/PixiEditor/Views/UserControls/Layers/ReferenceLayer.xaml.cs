using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PixiEditor.Models.IO;

namespace PixiEditor.Views.UserControls.Layers;

/// <summary>
/// Interaction logic for ReferenceLayer.xaml
/// </summary>
public partial class ReferenceLayer : UserControl
{
    /*public Layer Layer
    {
        get { return (Layer)GetValue(ReferenceLayerProperty); }
        set { SetValue(ReferenceLayerProperty, value); }
    }


    public static readonly DependencyProperty ReferenceLayerProperty =
        DependencyProperty.Register(nameof(Layer), typeof(Layer), typeof(ReferenceLayer), new PropertyMetadata(default(Layer)));*/


    public ReferenceLayer()
    {
        InitializeComponent();
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        /*string path = OpenFilePicker();
        if (path != null)
        {
            var bitmap = Importer.ImportSurface(path);
            Layer = new Layer("_Reference Layer", bitmap, bitmap.Width, bitmap.Height);
        }*/
    }

    private string OpenFilePicker()
    {
        var imagesFilter = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Images).GetFormattedTypes();
        OpenFileDialog dialog = new OpenFileDialog
        {
            Title = "Reference layer path",
            CheckPathExists = true,
            Filter = imagesFilter
        };

        return (bool)dialog.ShowDialog() ? dialog.FileName : null;
    }

    private void TrashButton_Click(object sender, RoutedEventArgs e)
    {
        //Layer = null;
    }
}
