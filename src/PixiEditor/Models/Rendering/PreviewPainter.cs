using Avalonia;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.Models.Rendering;

public class PreviewPainter
{
    public string ElementToRenderName { get; set; }
    public IPreviewRenderable PreviewRenderable { get; set; }
    public ColorSpace ProcessingColorSpace { get; set; }
    public event Action RequestRepaint;
    public KeyFrameTime FrameTime { get; set; }
    public VecI DocumentSize { get; set; }
    
    private Texture renderTexture;
    
    public PreviewPainter(IPreviewRenderable previewRenderable, KeyFrameTime frameTime, VecI documentSize, ColorSpace processingColorSpace, string elementToRenderName = "")
    {
        PreviewRenderable = previewRenderable;
        ElementToRenderName = elementToRenderName;
        ProcessingColorSpace = processingColorSpace;
        FrameTime = frameTime;
        DocumentSize = documentSize;
    }

    public void Paint(DrawingSurface renderOn, VecI boundsSize) 
    {
        if (PreviewRenderable == null)
        {
            return;
        }

        if (renderTexture == null || renderTexture.Size != boundsSize)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(boundsSize, ProcessingColorSpace);
        }
        
        renderTexture.DrawingSurface.Canvas.Clear();
        
        RenderContext context = new(renderTexture.DrawingSurface, FrameTime, ChunkResolution.Full, DocumentSize, ProcessingColorSpace);

        PreviewRenderable.RenderPreview(renderTexture.DrawingSurface, context, ElementToRenderName);
        
        renderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
    }

    public void Repaint()
    {
        RequestRepaint?.Invoke();
    }
}
