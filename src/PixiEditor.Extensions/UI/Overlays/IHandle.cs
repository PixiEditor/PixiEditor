using Avalonia.Media;

namespace PixiEditor.Extensions.UI.Overlays;

public interface IHandle
{
    public IOverlay Owner { get; }
    public IBrush HandleBrush { get; set; }
    public IPen? HandlePen { get; set; }
    public double ZoomScale { get; set; }

    public void Draw(DrawingContext context);
    protected void OnPressed(OverlayPointerArgs args);
}
