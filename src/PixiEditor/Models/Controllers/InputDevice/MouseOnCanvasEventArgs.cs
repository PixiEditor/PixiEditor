using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Controllers.InputDevice;

internal class MouseOnCanvasEventArgs : EventArgs
{
    public MouseButton Button { get; }
    public PointerType PointerType { get; }
    public KeyModifiers KeyModifiers { get; }
    public PointerPosition Point { get; }
    public bool Handled { get; set; }
    public int ClickCount { get; set; } = 1;
    public double ViewportScale { get; set; }
    public IReadOnlyList<PointerPosition> IntermediatePoints { get; set; }
    public PointerPointProperties Properties => Point.Properties;

    public MouseOnCanvasEventArgs(MouseButton button, PointerType type, VecD positionOnCanvas, KeyModifiers keyModifiers, int clickCount,
        PointerPointProperties properties, double viewportScale)
    {
        Button = button;
        Point = new PointerPosition(positionOnCanvas, properties);
        KeyModifiers = keyModifiers;
        ClickCount = clickCount;
        PointerType = type;
        ViewportScale = viewportScale;
    }

    public static MouseOnCanvasEventArgs FromIntermediatePoint(MouseOnCanvasEventArgs args, PointerPosition point)
    {
        return new MouseOnCanvasEventArgs(args.Button, args.PointerType, point.PositionOnCanvas, args.KeyModifiers, args.ClickCount,
            point.Properties, args.ViewportScale)
        {
            Handled = args.Handled,
        };
    }
}

struct PointerPosition
{
    public VecD PositionOnCanvas { get; set; }
    public PointerPointProperties Properties { get; set; }

    public PointerPosition(VecD positionOnCanvas, PointerPointProperties properties)
    {
        PositionOnCanvas = positionOnCanvas;
        Properties = properties;
    }
}
