using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Extensions.Palettes;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;
using BackendColors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.UserControls.Palettes;

internal partial class ColorReplacer : UserControl
{
    public PaletteColor ColorToReplace
    {
        get { return (PaletteColor)GetValue(ColorToReplaceProperty); }
        set { SetValue(ColorToReplaceProperty, value); }
    }

    public static readonly DependencyProperty ColorToReplaceProperty =
        DependencyProperty.Register(nameof(ColorToReplace), typeof(PaletteColor), typeof(ColorReplacer), new PropertyMetadata(PaletteColor.White));


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

    public static readonly DependencyProperty ReplaceColorsCommandProperty = DependencyProperty.Register(nameof(ReplaceColorsCommand), typeof(ICommand), typeof(ColorReplacer), new PropertyMetadata(default(ICommand)));

    public ICommand ReplaceColorsCommand
    {
        get { return (ICommand)GetValue(ReplaceColorsCommandProperty); }
        set { SetValue(ReplaceColorsCommandProperty, value); }
    }

    public static readonly DependencyProperty NewColorProperty =
        DependencyProperty.Register(nameof(NewColor), typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Black));
    
    public static readonly DependencyProperty HintColorProperty =
        DependencyProperty.Register(nameof(HintColor), typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Black));

    public bool IsCollapsed
    {
        get { return (bool)GetValue(IsCollapsedProperty); }
        set { SetValue(IsCollapsedProperty, value); }
    }

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(ColorReplacer), new PropertyMetadata(false));

    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(PaletteColorControl.PaletteColorDaoFormat))
        {
            string hex = (string)e.Data.GetData(PaletteColorControl.PaletteColorDaoFormat);
            if (hex is null)
            {
                return;
            }

            ColorToReplace = PaletteColor.Parse(hex);
        }
    }

    public ColorReplacer()
    {
        InitializeComponent();
    }

    private void ReplaceButton_OnClick(object sender, RoutedEventArgs e)
    {
        PaletteColor first = ColorToReplace;
        Color rawSecond = NewColor;

        PaletteColor second = new PaletteColor(rawSecond.R, rawSecond.G, rawSecond.B);

        var pack = (first, second);
        if (ReplaceColorsCommand.CanExecute(pack))
        {
            ReplaceColorsCommand.Execute(pack);
        }

        ColorToReplace = second;
        NewColor = new Color { R = first.R, G = first.G, B = first.B, A = 255 };
    }
}
