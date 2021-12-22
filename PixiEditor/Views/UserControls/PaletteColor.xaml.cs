using System.Windows;
using System.Windows.Controls;
using SkiaSharp;

namespace PixiEditor.Views.UserControls;

public partial class PaletteColor : UserControl
{
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        "Color", typeof(SKColor), typeof(PaletteColor), new PropertyMetadata(default(SKColor)));

    public SKColor Color
    {
        get { return (SKColor)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }

    public PaletteColor()
    {
        InitializeComponent();
    }
}