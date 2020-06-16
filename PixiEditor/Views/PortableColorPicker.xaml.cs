using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for PortableColorPicker.xaml
    /// </summary>
    public partial class PortableColorPicker : UserControl
    {
        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(PortableColorPicker),
                new PropertyMetadata(Colors.Black));

        // Using a DependencyProperty as the backing store for SecondaryColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryColorProperty =
            DependencyProperty.Register("SecondaryColor", typeof(Color), typeof(PortableColorPicker),
                new PropertyMetadata(Colors.White));

        public PortableColorPicker()
        {
            InitializeComponent();
        }


        public Color SelectedColor
        {
            get => (Color) GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }


        public Color SecondaryColor
        {
            get => (Color) GetValue(SecondaryColorProperty);
            set => SetValue(SecondaryColorProperty, value);
        }
    }
}