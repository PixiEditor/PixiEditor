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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PixiEditor.Models.Layers;

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

        public LayerStructureItemContainer()
        {
            InitializeComponent();
        }
    }
}