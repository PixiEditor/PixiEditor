using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Views.UserControls.Palettes
{
    /// <summary>
    /// Interaction logic for ColorReplacer.xaml
    /// </summary>
    public partial class ColorReplacer : UserControl
    {

        public Color HintColor
        {
            get { return (Color)GetValue(HintColorProperty); }
            set { SetValue(HintColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintColorProperty =
            DependencyProperty.Register("HintColor", typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Transparent));

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public ColorReplacer()
        {
            InitializeComponent();
        }
    }
}
