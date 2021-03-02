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

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayersManager.xaml.
    /// </summary>
    public partial class LayersManager : UserControl
    {

        public LayersManager LayersViewModel
        {
            get { return (LayersManager)GetValue(LayersViewModelProperty); }
            set { SetValue(LayersViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayersViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersViewModelProperty =
            DependencyProperty.Register("LayersViewModel", typeof(LayersManager), typeof(LayersManager), new PropertyMetadata(0));

        public LayersManager()
        {
            InitializeComponent();
        }
    }
}