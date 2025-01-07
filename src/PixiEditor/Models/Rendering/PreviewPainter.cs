using Avalonia;
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
    public VecI SizeToRequest { get; set; }

    private int renderRequestId;
    //private Texture renderTexture;

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

    public void Paint(DrawingSurface renderOn, VecI boundsSize, Matrix3X3 matrix)
    {
        if (PreviewRenderable?.LastRenderedPreviews == null)
        {
            return;
        }

        if (PreviewRenderable.LastRenderedPreviews.TryGetValue(renderRequestId, out Image preview))
        {
            renderOn.Canvas.Clear();
            int saved = renderOn.Canvas.Save();
            
            Matrix3X3 targetMatrix = ScaleToFitUniform(new RectD(0, 0, preview.Size.X, preview.Size.Y), SizeToRequest);

            renderOn.Canvas.SetMatrix(targetMatrix);

            renderOn.Canvas.DrawImage(preview, 0, 0);

            renderOn.Canvas.RestoreToCount(saved);
        }

        /*if (renderTexture == null || renderTexture.Size != boundsSize)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(boundsSize, ProcessingColorSpace);
        }

        renderTexture.DrawingSurface.Canvas.Clear();
        renderTexture.DrawingSurface.Canvas.Save();

        renderTexture.DrawingSurface.Canvas.SetMatrix(matrix);

        RenderContext context = new(renderTexture.DrawingSurface, FrameTime, ChunkResolution.Full, DocumentSize, ProcessingColorSpace);

        Renderer.RenderNodePreview(PreviewRenderable, renderTexture.DrawingSurface, context, ElementToRenderName);
        renderTexture.DrawingSurface.Canvas.Restore();

        renderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);*/
    }

    public void Repaint()
    {
        if (SizeToRequest.X == 0 || SizeToRequest.Y == 0)
        {
            return;
        }

        renderRequestId = Renderer.QueueRenderPreview(SizeToRequest, PreviewRenderable.RenderableId,
            ElementToRenderName, FrameTime, OnRendered);
    }

    private void OnRendered()
    {
        RequestRepaint?.Invoke();
    }

    private Matrix3X3 ScaleToFitUniform(RectD bounds, VecI size)
    {
        float scaleX = (float)size.X / (float)bounds.Width;
        float scaleY = (float)size.Y / (float)bounds.Height;
        float scale = Math.Min(scaleX, scaleY);
        float dX = (float)size.X / 2 / scale - (float)bounds.Width / 2;
        dX -= (float)bounds.X;
        float dY = (float)size.Y / 2 / scale - (float)bounds.Height / 2;
        dY -= (float)bounds.Y;
        Matrix3X3 matrix = Matrix3X3.CreateScale(scale, scale);
        return matrix.Concat(Matrix3X3.CreateTranslation(dX, dY));
    }
}
