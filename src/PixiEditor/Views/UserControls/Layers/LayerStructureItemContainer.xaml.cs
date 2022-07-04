using PixiEditor.Models.Layers;
using PixiEditor.ViewModels.SubViewModels.Main;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views.UserControls.Layers;

/// <summary>
/// Interaction logic for LayerStructureItemContainer.xaml.
/// </summary>
public partial class LayerStructureItemContainer : UserControl
{
    public Layer Layer
    {
        get { return (Layer)GetValue(LayerProperty); }
        set { SetValue(LayerProperty, value); }
    }


    public static readonly DependencyProperty LayerProperty =
        DependencyProperty.Register(nameof(Layer), typeof(Layer), typeof(LayerStructureItemContainer), new PropertyMetadata(default(Layer)));

    public LayersViewModel LayerCommandsViewModel
    {
        get { return (LayersViewModel)GetValue(LayerCommandsViewModelProperty); }
        set { SetValue(LayerCommandsViewModelProperty, value); }
    }


    public static readonly DependencyProperty LayerCommandsViewModelProperty =
        DependencyProperty.Register(nameof(LayerCommandsViewModel), typeof(LayersViewModel), typeof(LayerStructureItemContainer), new PropertyMetadata(default(LayersViewModel)));

    public int ContainerIndex
    {
        get { return (int)GetValue(ContainerIndexProperty); }
        set { SetValue(ContainerIndexProperty, value); }
    }


    public static readonly DependencyProperty ContainerIndexProperty =
        DependencyProperty.Register(nameof(ContainerIndex), typeof(int), typeof(LayerStructureItemContainer), new PropertyMetadata(0));

    public LayerStructureItemContainer()
    {
        InitializeComponent();
    }
}