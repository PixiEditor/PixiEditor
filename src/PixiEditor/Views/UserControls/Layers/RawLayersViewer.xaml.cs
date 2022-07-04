using PixiEditor.Models.Layers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views.UserControls.Layers;

/// <summary>
/// Interaction logic for RawLayersViewer.xaml.
/// </summary>
public partial class RawLayersViewer : UserControl
{
    public ObservableCollection<Layer> Layers
    {
        get { return (ObservableCollection<Layer>)GetValue(LayersProperty); }
        set { SetValue(LayersProperty, value); }
    }


    public static readonly DependencyProperty LayersProperty =
        DependencyProperty.Register(nameof(Layers),
            typeof(ObservableCollection<Layer>),
            typeof(RawLayersViewer),
            new PropertyMetadata(default(ObservableCollection<Layer>)));

    public LayerStructure Structure
    {
        get { return (LayerStructure)GetValue(StructureProperty); }
        set { SetValue(StructureProperty, value); }
    }


    public static readonly DependencyProperty StructureProperty =
        DependencyProperty.Register(nameof(Structure), typeof(LayerStructure), typeof(RawLayersViewer), new PropertyMetadata(default(LayerStructure)));

    public RawLayersViewer()
    {
        InitializeComponent();
    }
}