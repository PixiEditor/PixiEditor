using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class RenderNode : Node, IPreviewRenderable, IHighDpiRenderNode
{
    public RenderOutputProperty Output { get; }

    public Dictionary<int, Image> LastRenderedPreviews { get; private set; }
    Guid IPreviewRenderable.RenderableId => Id;
    public bool AllowHighDpiRendering { get; set; } = false;

    private TextureCache textureCache = new();

    public RenderNode()
    {
        Painter painter = new Painter(Paint);
        Output = CreateRenderOutput("Output", "OUTPUT",
            () => painter,
            () => this is IRenderInput renderInput ? renderInput.Background.Value : null);
    }

    protected override void OnExecute(RenderContext context)
    {
        foreach (var prop in OutputProperties)
        {
            if (prop is RenderOutputProperty output)
            {
                output.ChainToPainterValue();
            }
        }
    }

    private void Paint(RenderContext context, DrawingSurface surface)
    {
        DrawingSurface target = surface;
        bool useIntermediate = !AllowHighDpiRendering
                               && context.DocumentSize is { X: > 0, Y: > 0 }
                               && surface.DeviceClipBounds.Size != context.DocumentSize;
        if (useIntermediate)
        {
            Texture intermediate = textureCache.RequestTexture(0, context.DocumentSize, context.ProcessingColorSpace);
            target = intermediate.DrawingSurface;
        }

        OnPaint(context, target);

        if (useIntermediate)
        {
            surface.Canvas.DrawSurface(target, 0, 0);
        }

        if (context.PendingPreviewRequests != null && context.PendingPreviewRequests.TryGetValue(Id, out var requests))
        {
            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                RectD? bounds = GetPreviewBounds(request.Frame.Frame, request.ElementName);
                if (bounds != null)
                {
                    using Texture previewSurface = Texture.ForProcessing(request.Size);
                    previewSurface.DrawingSurface.Canvas.Save();
                    
                    UniformScale(bounds.Value, request.Size, previewSurface.DrawingSurface.Canvas);
                    
                    if (RenderPreview(previewSurface.DrawingSurface, context, request.ElementName))
                    {
                        if(LastRenderedPreviews == null)
                        {
                            LastRenderedPreviews = new Dictionary<int, Image>();
                        }
                        
                        if (LastRenderedPreviews.ContainsKey(request.Id))
                        {
                            LastRenderedPreviews[request.Id].Dispose();
                            LastRenderedPreviews[request.Id] = previewSurface.DrawingSurface.Snapshot();
                        }
                        else
                        {
                            LastRenderedPreviews.Add(request.Id, previewSurface.DrawingSurface.Snapshot());
                        }
                    }
                    
                    previewSurface.DrawingSurface.Canvas.Restore();
                }
            }
        }
    }
    
    private void UniformScale(RectD bounds, VecI size, Canvas canvas)
    {
        float scaleX = (float)size.X / (float)bounds.Width;
        float scaleY = (float)size.Y / (float)bounds.Height;
        float scale = Math.Min(scaleX, scaleY);
        float dX = (float)size.X / 2 / scale - (float)bounds.Width / 2;
        dX -= (float)bounds.X;
        float dY = (float)size.Y / 2 / scale - (float)bounds.Height / 2;
        dY -= (float)bounds.Y;
        Matrix3X3 matrix = Matrix3X3.CreateScale(scale, scale);
        matrix = matrix.Concat(Matrix3X3.CreateTranslation(dX, dY));
        canvas.SetMatrix(matrix);
    }

    protected abstract void OnPaint(RenderContext context, DrawingSurface surface);

    public abstract RectD? GetPreviewBounds(int frame, string elementToRenderName = "");

    public abstract bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName);

    protected Texture RequestTexture(int id, VecI size, ColorSpace processingCs, bool clear = true)
    {
        return textureCache.RequestTexture(id, size, processingCs, clear);
    }

    public override void Dispose()
    {
        base.Dispose();
        textureCache.Dispose();
    }
}
