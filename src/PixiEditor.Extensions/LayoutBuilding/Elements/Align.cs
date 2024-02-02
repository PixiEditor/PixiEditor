using Avalonia.Controls;
using Avalonia.Layout;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Align : SingleChildLayoutElement
{
    public Alignment Alignment { get; set; }

    public Align(LayoutElement child = null, Alignment alignment = Alignment.Center)
    {
        Child = child;
        Alignment = alignment;
    }

    public override Control BuildNative()
    {
        Panel panel = new Panel
        {
            HorizontalAlignment = DecomposeHorizontalAlignment(Alignment), VerticalAlignment = DecomposeVerticalAlignment(Alignment)
        };

        if (Child != null)
        {
            panel.Children.Add(Child.BuildNative());
        }

        return panel;
    }

    private HorizontalAlignment DecomposeHorizontalAlignment(Alignment alignment)
    {
        return alignment switch
        {
            Alignment.TopLeft => HorizontalAlignment.Left,
            Alignment.TopCenter => HorizontalAlignment.Center,
            Alignment.TopRight => HorizontalAlignment.Right,
            Alignment.CenterLeft => HorizontalAlignment.Left,
            Alignment.Center => HorizontalAlignment.Center,
            Alignment.CenterRight => HorizontalAlignment.Right,
            Alignment.BottomLeft => HorizontalAlignment.Left,
            Alignment.BottomCenter => HorizontalAlignment.Center,
            Alignment.BottomRight => HorizontalAlignment.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
        };
    }

    private VerticalAlignment DecomposeVerticalAlignment(Alignment alignment)
    {
        return alignment switch
        {
            Alignment.TopLeft => VerticalAlignment.Top,
            Alignment.TopCenter => VerticalAlignment.Top,
            Alignment.TopRight => VerticalAlignment.Top,
            Alignment.CenterLeft => VerticalAlignment.Center,
            Alignment.Center => VerticalAlignment.Center,
            Alignment.CenterRight => VerticalAlignment.Center,
            Alignment.BottomLeft => VerticalAlignment.Bottom,
            Alignment.BottomCenter => VerticalAlignment.Bottom,
            Alignment.BottomRight => VerticalAlignment.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
        };
    }
}

public enum Alignment
{
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    Center,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}
