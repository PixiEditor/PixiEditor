using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Layout;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Align : SingleChildLayoutElement, IPropertyDeserializable
{
    private Panel _panel; 
    public Alignment Alignment { get; set; }

    public Align(LayoutElement child = null, Alignment alignment = Alignment.Center)
    {
        Child = child;
        Alignment = alignment;
    }

    protected override Control CreateNativeControl()
    {
        _panel = new Panel
        {
            HorizontalAlignment = DecomposeHorizontalAlignment(Alignment), VerticalAlignment = DecomposeVerticalAlignment(Alignment)
        };

        if (Child != null)
        {
            _panel.Children.Add(Child.BuildNative());
        }

        return _panel;
    }

    protected override void AddChild(Control child)
    {
        _panel.Children.Add(child);
    }

    protected override void RemoveChild()
    {
        _panel.Children.Clear(); 
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

    public void DeserializeProperties(ImmutableList<object> values)
    {
        Alignment = (Alignment)values.FirstOrDefault();
    }
    
    IEnumerable<object> IPropertyDeserializable.GetProperties()
    {
        yield return Alignment;
    }
}
