using Avalonia;
using Avalonia.Controls.Primitives;
using Drawie.Numerics;

namespace PixiEditor.Views.Nodes;

public class NodeFrameView : TemplatedControl
{
    public static readonly StyledProperty<VecD> TopLeftProperty = AvaloniaProperty.Register<ConnectionLine, VecD>(nameof(TopLeft));
    
    public VecD TopLeft
    {
        get => GetValue(TopLeftProperty);
        set => SetValue(TopLeftProperty, value);
    }
    
    public static readonly StyledProperty<VecD> BottomRightProperty = AvaloniaProperty.Register<ConnectionLine, VecD>(nameof(BottomRight));
    
    public VecD BottomRight
    {
        get => GetValue(BottomRightProperty);
        set => SetValue(BottomRightProperty, value);
    }
    
    public static readonly StyledProperty<VecD> SizeProperty = AvaloniaProperty.Register<ConnectionLine, VecD>(nameof(Size));
    
    public VecD Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
