using Avalonia;
using Avalonia.Media;
using PixiEditor.Models.Rendering;

namespace PixiEditor.Views.Visuals;

public class PreviewPainterImage : IImage
{
    public PreviewPainter PreviewPainter { get; set; }
    
    public int FrameToRender { get; set; }
    public Size Size => new Size(
        PreviewPainter.PreviewRenderable.GetPreviewBounds(FrameToRender)?.Size.X ?? 0,
        PreviewPainter.PreviewRenderable.GetPreviewBounds(FrameToRender)?.Size.Y ?? 0); 
    
    public PreviewPainterImage(PreviewPainter previewPainter, int frameToRender)
    {
        PreviewPainter = previewPainter;
        FrameToRender = frameToRender;
    }
    
    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        if (PreviewPainter.PreviewRenderable.GetPreviewBounds(FrameToRender) == null) return;
        
        context.PushClip(destRect);
        using DrawPreviewOperation drawPreviewOperation = new DrawPreviewOperation(destRect, PreviewPainter, FrameToRender); 
        context.Custom(drawPreviewOperation);
    }
}
