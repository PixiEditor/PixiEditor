using PixiEditor.Models.DataHolders;
using SkiaSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls.Palettes
{
    /// <summary>
    /// Interaction logic for CompactPaletteViewer.xaml
    /// </summary>
    public partial class CompactPaletteViewer : UserControl
    {
        public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(nameof(Colors), typeof(WpfObservableRangeCollection<SKColor>), typeof(CompactPaletteViewer));

        public WpfObservableRangeCollection<SKColor> Colors
        {
            get { return (WpfObservableRangeCollection<SKColor>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        public ICommand SelectColorCommand
        {
            get { return (ICommand)GetValue(SelectColorCommandProperty); }
            set { SetValue(SelectColorCommandProperty, value); }
        }


        public static readonly DependencyProperty SelectColorCommandProperty =
            DependencyProperty.Register(nameof(SelectColorCommand), typeof(ICommand), typeof(CompactPaletteViewer));

        public CompactPaletteViewer()
        {
            InitializeComponent();
        }

        private void RemoveColorMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            SKColor color = (SKColor)menuItem.CommandParameter;
            if (Colors.Contains(color))
            {
                Colors.Remove(color);
            }
        }
    }
}
