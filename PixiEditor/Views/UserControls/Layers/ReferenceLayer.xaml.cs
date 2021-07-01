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

namespace PixiEditor.Views.UserControls.Layers
{
    /// <summary>
    /// Interaction logic for ReferenceLayer.xaml
    /// </summary>
    public partial class ReferenceLayer : UserControl
    {
        public WriteableBitmap PreviewImage
        {
            get { return (WriteableBitmap)GetValue(PreviewImageProperty); }
            set { SetValue(PreviewImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PreviewImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewImageProperty =
            DependencyProperty.Register("PreviewImage", typeof(WriteableBitmap), typeof(ReferenceLayer), new PropertyMetadata(default(WriteableBitmap)));


        public bool IsLayerVisible
        {
            get { return (bool)GetValue(IsLayerVisibleProperty); }
            set { SetValue(IsLayerVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLayerVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLayerVisibleProperty =
            DependencyProperty.Register("IsLayerVisible", typeof(bool), typeof(ReferenceLayer), new PropertyMetadata(true));


        public bool ReferenceLayerSelected
        {
            get { return (bool)GetValue(ReferenceLayerSelectedProperty); }
            set { SetValue(ReferenceLayerSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReferenceLayerSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReferenceLayerSelectedProperty =
            DependencyProperty.Register("ReferenceLayerSelected", typeof(bool), typeof(ReferenceLayer), new PropertyMetadata(false));



        public ReferenceLayer()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
