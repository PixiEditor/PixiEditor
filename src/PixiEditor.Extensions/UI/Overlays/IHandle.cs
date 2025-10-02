using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace PixiEditor.Extensions.UI.Overlays;

public interface IHandle
{
    public IOverlay Owner { get; }
    public Paint? FillPaint { get; set; }
    public Paint? StrokePaint { get; set; }
    public double ZoomScale { get; set; }

    protected void Draw(Canvas target);
    protected void OnPressed(OverlayPointerArgs args);
}
