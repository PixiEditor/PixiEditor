using Avalonia;
using Avalonia.Threading;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
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
    public DocumentRenderer Renderer { get; set; }
    public VecI Bounds { get; set; }
    public Matrix3X3 Matrix { get; set; }

    private Texture renderTexture;
    private bool requestedRepaint;

    public PreviewPainter(DocumentRenderer renderer, IPreviewRenderable previewRenderable, KeyFrameTime frameTime,
        VecI documentSize, ColorSpace processingColorSpace, string elementToRenderName = "")
    {
        PreviewRenderable = previewRenderable;
        ElementToRenderName = elementToRenderName;
        ProcessingColorSpace = processingColorSpace;
        FrameTime = frameTime;
        DocumentSize = documentSize;
        Renderer = renderer;
    }

    public void Paint(DrawingSurface renderOn)
    {
        if (renderTexture == null || renderTexture.IsDisposed)
        {
            return;
        }

        renderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
    }

    public void Repaint()
    {
        if (Bounds.ShortestAxis == 0 || requestedRepaint) return;

        requestedRepaint = true;
        
        if (renderTexture == null || renderTexture.Size != Bounds)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(Bounds, ProcessingColorSpace);
        }

        renderTexture.DrawingSurface.Canvas.Clear();
        renderTexture.DrawingSurface.Canvas.Save();

        renderTexture.DrawingSurface.Canvas.SetMatrix(Matrix);

        RenderContext context = new(renderTexture.DrawingSurface, FrameTime, ChunkResolution.Full, DocumentSize,
            ProcessingColorSpace);

        Renderer.RenderNodePreview(PreviewRenderable, renderTexture.DrawingSurface, context, ElementToRenderName)
            .ContinueWith(_ =>
            {
                renderTexture.DrawingSurface.Canvas.Restore();
                Dispatcher.UIThread.Invoke(() => RequestRepaint?.Invoke());
                requestedRepaint = false;
            });
    }
}
