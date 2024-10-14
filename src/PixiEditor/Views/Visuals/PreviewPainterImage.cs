using Avalonia;
using Avalonia.Media;
using PixiEditor.Models.Rendering;

namespace PixiEditor.Views.Visuals;

public class PreviewPainterImage : IImage
{
    public PreviewPainter PreviewPainter { get; set; }
    
    public int FrameToRender { get; set; }
    public Size Size => new Size(PreviewPainter.Bounds?.Size.X ?? 0, PreviewPainter.Bounds?.Size.Y ?? 0); 
    
    public PreviewPainterImage(PreviewPainter previewPainter, int frameToRender)
    {
        PreviewPainter = previewPainter;
        FrameToRender = frameToRender;
    }
    
    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        if (PreviewPainter.Bounds == null) return;
        
        using DrawPreviewOperation drawPreviewOperation = new DrawPreviewOperation(destRect, PreviewPainter, FrameToRender); 
        context.Custom(drawPreviewOperation);
    }
}
