using PixiEditor.Models.DataHolders;
using SkiaSharp;
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
    /// Interaction logic for PaletteColorAdder.xaml
    /// </summary>
    public partial class PaletteColorAdder : UserControl
    {
        public ObservableCollection<SKColor> Colors
        {
            get { return (ObservableCollection<SKColor>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(PaletteColorAdder), 
                new PropertyMetadata(System.Windows.Media.Colors.Black));


        // Using a DependencyProperty as the backing store for Colors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register("Colors", typeof(ObservableCollection<SKColor>), typeof(PaletteColorAdder), new PropertyMetadata(default(ObservableCollection<SKColor>), OnColorsChanged));

        private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PaletteColorAdder adder = (PaletteColorAdder)d;
            if (e.NewValue != null)
            {
                adder.Colors.CollectionChanged += adder.Colors_CollectionChanged;
            }
            else if(e.OldValue != null)
            {
                adder.Colors.CollectionChanged -= adder.Colors_CollectionChanged;
            }
        }

        private void Colors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AddButton.IsEnabled = !Colors.Contains(ToSKColor(SelectedColor));
        }

        public PaletteColorAdder()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SKColor color = ToSKColor(SelectedColor);
            if (!Colors.Contains(color))
            {
                Colors.Add(color);
                AddButton.IsEnabled = false;
            }
        }

        private void PortableColorPicker_ColorChanged(object sender, RoutedEventArgs e) => 
            AddButton.IsEnabled = !Colors.Contains(ToSKColor(SelectedColor));

        private static SKColor ToSKColor(Color color) => new SKColor(color.R, color.G, color.B, color.A);
    }
}
