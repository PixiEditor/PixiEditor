using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PixiEditor.Views.UserControls.Lospec
{
    /// <summary>
    /// Interaction logic for LospecPaletteItem.xaml
    /// </summary>
    public partial class LospecPaletteItem : UserControl
    {
        public ObservableCollection<SKColor> Colors
        {
            get { return (ObservableCollection<SKColor>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Colors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register("Colors", typeof(ObservableCollection<SKColor>), typeof(LospecPaletteItem));


        public LospecPaletteItem()
        {
            InitializeComponent();
        }
    }
}
