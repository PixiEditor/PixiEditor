using ChunkyImageLib.DataHolders;
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
    
    public PreviewPainter(IPreviewRenderable previewRenderable, KeyFrameTime frameTime, VecI documentSize, ColorSpace processingColorSpace, string elementToRenderName = "")
    {
        PreviewRenderable = previewRenderable;
        ElementToRenderName = elementToRenderName;
        ProcessingColorSpace = processingColorSpace;
        FrameTime = frameTime;
        DocumentSize = documentSize;
    }

    public void Paint(DrawingSurface renderOn) 
    {
        if (PreviewRenderable == null)
        {
            return;
        }

        RenderContext context = new(renderOn, FrameTime, ChunkResolution.Full, DocumentSize, ProcessingColorSpace);

        PreviewRenderable.RenderPreview(renderOn, context, ElementToRenderName);
    }

    public void Repaint()
    {
        RequestRepaint?.Invoke();
    }
}
