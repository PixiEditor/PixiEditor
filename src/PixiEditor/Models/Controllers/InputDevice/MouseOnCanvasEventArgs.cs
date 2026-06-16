using Avalonia.Input;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.Controllers.InputDevice;

internal class MouseOnCanvasEventArgs : EventArgs
{
    public MouseButton Button { get; }
    public PointerType PointerType { get; }
    public KeyModifiers KeyModifiers { get; }
    public PointerPosition Point { get; }
    public PointerInfo Info { get; }
    public bool Handled { get; set; }
    public int ClickCount { get; set; } = 1;
    public double ViewportScale { get; set; }
    public IReadOnlyList<PointerPosition> IntermediatePoints { get; set; }
    public PointerPointProperties Properties => Point.Properties;
    public IDocument? TargetDocument { get; set; }

    public MouseOnCanvasEventArgs(MouseButton button, PointerType type, PointerInfo info, KeyModifiers keyModifiers, int clickCount,
        PointerPointProperties properties, double viewportScale, IDocument? targetDocument)
    {
        Button = button;
        Info = info;
        Point = new PointerPosition(info.PositionOnCanvas, properties);
        KeyModifiers = keyModifiers;
        ClickCount = clickCount;
        PointerType = type;
        ViewportScale = viewportScale;
        TargetDocument = targetDocument;
    }

    public static MouseOnCanvasEventArgs FromIntermediatePoint(MouseOnCanvasEventArgs args, PointerPosition point)
    {
        var pointerInfo = args.Info with
        {
            PositionOnCanvas = point.PositionOnCanvas,
        };

        return new MouseOnCanvasEventArgs(args.Button, args.PointerType, pointerInfo, args.KeyModifiers, args.ClickCount,
            point.Properties, args.ViewportScale, args.TargetDocument)
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
