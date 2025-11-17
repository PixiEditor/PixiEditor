using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Markup.Xaml;
using PixiEditor.Models.BrushEngine;

namespace PixiEditor.Views.Input;

internal partial class BrushItem : UserControl
{
    public static readonly StyledProperty<Brush> BrushProperty = AvaloniaProperty.Register<BrushItem, Brush>("Brush");

    public BrushItem()
    {
        InitializeComponent();
    }

    public Brush Brush
    {
        get { return (Brush)GetValue(BrushProperty); }
        set { SetValue(BrushProperty, value); }
    }
}

