using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Drawie.Numerics;

namespace PixiEditor.Views.Nodes;

public class NodeFrameView : TemplatedControl
{
    public static readonly StyledProperty<StreamGeometry> GeometryProperty = AvaloniaProperty.Register<NodeFrameView, StreamGeometry>(nameof(Geometry));

    public StreamGeometry Geometry
    {
        get => GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }
}
