using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayerStructureItemContainer.xaml.
    /// </summary>
    public partial class LayerStructureItemContainer : UserControl
    {
        public LayerStructureItem Item
        {
            get { return (LayerStructureItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(
                "Item",
                typeof(LayerStructureItem),
                typeof(LayerStructureItemContainer),
                new PropertyMetadata(default(LayerStructureItem)));

        public LayersViewModel LayerCommandsViewModel
        {
            get { return (LayersViewModel)GetValue(LayerCommandsViewModelProperty); }
            set { SetValue(LayerCommandsViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerCommandsViewModelProperty =
            DependencyProperty.Register("LayerCommandsViewModel", typeof(LayersViewModel), typeof(LayerStructureItemContainer), new PropertyMetadata(default(LayersViewModel)));

        public int ContainerIndex
        {
            get { return (int)GetValue(ContainerIndexProperty); }
            set { SetValue(ContainerIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContainerIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContainerIndexProperty =
            DependencyProperty.Register("ContainerIndex", typeof(int), typeof(LayerStructureItemContainer), new PropertyMetadata(0));

        public LayerStructureItemContainer()
        {
            InitializeComponent();
        }
    }
}