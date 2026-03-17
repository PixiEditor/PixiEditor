using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Scroll : SingleChildLayoutElement
{
    private ScrollViewer scrollViewer;
    public ScrollDirection Direction { get; set; } = ScrollDirection.Vertical;

    protected override Control CreateNativeControl()
    {
        scrollViewer = new();
        scrollViewer.HorizontalScrollBarVisibility = Direction == ScrollDirection.Horizontal
            ? ScrollBarVisibility.Auto
            : ScrollBarVisibility.Disabled;
        scrollViewer.VerticalScrollBarVisibility = Direction == ScrollDirection.Vertical
            ? ScrollBarVisibility.Auto
            : ScrollBarVisibility.Disabled;

        if (Child != null)
        {
            scrollViewer.Content = Child.BuildNative();
        }

        return scrollViewer;
    }

    protected override void AddChild(Control child)
    {
        scrollViewer.Content = child;
    }

    protected override void RemoveChild()
    {
        scrollViewer.Content = null;
    }

    protected override IEnumerable<object> GetControlProperties()
    {
        yield return Direction;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        Direction = (ScrollDirection)values[0];
    }
}
