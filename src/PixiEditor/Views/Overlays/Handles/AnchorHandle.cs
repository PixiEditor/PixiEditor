using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Numerics;

namespace PixiEditor.Views.Overlays.Handles;

public class AnchorHandle : RectangleHandle
{
    private Pen pen;
    public AnchorHandle(Overlay owner) : base(owner)
    {
        Size = new VecD(GetResource<double>("AnchorHandleSize"));
        pen = new Pen(GetResource<SolidColorBrush>("HandleBrush"));
        HandlePen = pen;
    }

    public override void Draw(DrawingContext context)
    {
        pen.Thickness = 1.0 / ZoomScale;
        base.Draw(context);
    }
}
