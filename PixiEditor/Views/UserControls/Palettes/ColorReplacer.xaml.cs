using SkiaSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Views.UserControls.Palettes
{
    public partial class ColorReplacer : UserControl
    {
        public SKColor ColorToReplace
        {
            get { return (SKColor)GetValue(ColorToReplaceProperty); }
            set { SetValue(ColorToReplaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorToReplace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorToReplaceProperty =
            DependencyProperty.Register("ColorToReplace", typeof(SKColor), typeof(ColorReplacer), new PropertyMetadata(SKColors.White));


        public Color HintColor
        {
            get { return (Color)GetValue(HintColorProperty); }
            set { SetValue(HintColorProperty, value); }
        }

        public Color NewColor
        {
            get { return (Color)GetValue(NewColorProperty); }
            set { SetValue(NewColorProperty, value); }
        }

        public static readonly DependencyProperty ReplaceColorsCommandProperty = DependencyProperty.Register(
            "ReplaceColorsCommand", typeof(ICommand), typeof(ColorReplacer), new PropertyMetadata(default(ICommand)));

        public ICommand ReplaceColorsCommand
        {
            get { return (ICommand)GetValue(ReplaceColorsCommandProperty); }
            set { SetValue(ReplaceColorsCommandProperty, value); }
        }


        // Using a DependencyProperty as the backing store for NewColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewColorProperty =
            DependencyProperty.Register("NewColor", typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Black));


        // Using a DependencyProperty as the backing store for HintColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintColorProperty =
            DependencyProperty.Register("HintColor", typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Black));



        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCollapsed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(ColorReplacer), new PropertyMetadata(false));



        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(PaletteColor.PaletteColorDaoFormat))
            {
                string hex = (string)e.Data.GetData(PaletteColor.PaletteColorDaoFormat);
                ColorToReplace = SKColor.Parse(hex).WithAlpha(255);
            }
        }

        public ColorReplacer()
        {
            InitializeComponent();
        }

        private void ReplaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            SKColor first = ColorToReplace.WithAlpha(255);
            Color rawSecond = NewColor;

            SKColor second = new SKColor(rawSecond.R, rawSecond.G, rawSecond.B, 255);

            var pack = (first, second);
            if (ReplaceColorsCommand.CanExecute(pack))
            {
                ReplaceColorsCommand.Execute(pack);
            }

            ColorToReplace = SKColors.White;
        }
    }
}
