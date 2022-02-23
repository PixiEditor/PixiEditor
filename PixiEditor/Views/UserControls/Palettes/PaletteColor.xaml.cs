using System.Windows;
using System.Windows.Controls;
using SkiaSharp;

namespace PixiEditor.Views.UserControls.Palettes;

public partial class PaletteColor : UserControl
{
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        "Color", typeof(SKColor), typeof(PaletteColor), new PropertyMetadata(default(SKColor)));

    public SKColor Color
    {
        get { return (SKColor)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }


    public int? AssociatedKey
    {
        get { return (int?)GetValue(AssociatedKeyProperty); }
        set { SetValue(AssociatedKeyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for AssociatedKey.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty AssociatedKeyProperty =
        DependencyProperty.Register("AssociatedKey", typeof(int?), typeof(PaletteColor), new PropertyMetadata(null));


    public PaletteColor()
    {
        InitializeComponent();
    }
}