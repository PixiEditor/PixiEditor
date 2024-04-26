using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class RectangleHandle : Handle
{
    public double AnchorRadius { get; set; } = GetResource<double>("AnchorRadius");
    public RectangleHandle(Overlay owner) : base(owner)
    {
    }

    public override void Draw(DrawingContext context)
    {
        double scaleMultiplier = (1.0 / ZoomScale);
        double radius = AnchorRadius * scaleMultiplier;
        context.DrawRectangle(HandleBrush, HandlePen, TransformHelper.ToHandleRect(Position, Size, ZoomScale), radius, radius);
    }
}
