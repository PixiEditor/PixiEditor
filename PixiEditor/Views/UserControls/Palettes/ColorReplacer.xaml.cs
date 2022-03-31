using SkiaSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Views.UserControls.Palettes
{
    /// <summary>
    /// Interaction logic for ColorReplacer.xaml
    /// </summary>
    public partial class ColorReplacer : UserControl
    {
        public SKColor ColorToReplace
        {
            get { return (SKColor)GetValue(ColorToReplaceProperty); }
            set { SetValue(ColorToReplaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorToReplace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorToReplaceProperty =
            DependencyProperty.Register("ColorToReplace", typeof(SKColor), typeof(ColorReplacer), new PropertyMetadata(SKColors.Transparent));


        public Color HintColor
        {
            get { return (Color)GetValue(HintColorProperty); }
            set { SetValue(HintColorProperty, value); }
        }

        public Color NewColor
        {
            get { return (Color) GetValue(NewColorProperty); }
            set { SetValue(NewColorProperty, value); }
        }

        public static readonly DependencyProperty ReplaceColorsCommandProperty = DependencyProperty.Register(
            "ReplaceColorsCommand", typeof(ICommand), typeof(ColorReplacer), new PropertyMetadata(default(ICommand)));

        public ICommand ReplaceColorsCommand
        {
            get { return (ICommand) GetValue(ReplaceColorsCommandProperty); }
            set { SetValue(ReplaceColorsCommandProperty, value); }
        }


        // Using a DependencyProperty as the backing store for NewColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewColorProperty =
            DependencyProperty.Register("NewColor", typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Black));


        // Using a DependencyProperty as the backing store for HintColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintColorProperty =
            DependencyProperty.Register("HintColor", typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Transparent));

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(PaletteViewer.PaletteColorDaoFormat))
            {
                string hex = (string)e.Data.GetData(PaletteViewer.PaletteColorDaoFormat);
                ColorToReplace = SKColor.Parse(hex);
            }
        }

        public ColorReplacer()
        {
            InitializeComponent();
        }
    }
}
