using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Structure;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class RenderNode : Node, IPreviewRenderable, IHighDpiRenderNode
{
    public RenderOutputProperty Output { get; }

    public bool AllowHighDpiRendering { get; set; } = false;

    private TextureCache textureCache = new();

    private VecI lastDocumentSize = VecI.Zero;

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

        lastDocumentSize = context.DocumentSize;
    }

    private void Paint(RenderContext context, DrawingSurface surface)
    {
        DrawingSurface target = surface;
        bool useIntermediate = !AllowHighDpiRendering
                               && context.DocumentSize is { X: > 0, Y: > 0 }
                               && surface.DeviceClipBounds.Size != context.DocumentSize;
        if (useIntermediate)
        {
            Texture intermediate = textureCache.RequestTexture(-6451, context.DocumentSize, context.ProcessingColorSpace);

            int saved = intermediate.DrawingSurface.Canvas.Save();
            Matrix3X3 fitMatrix = Matrix3X3.CreateScale(
                (float)context.DocumentSize.X / surface.DeviceClipBounds.Size.X,
                (float)context.DocumentSize.Y / surface.DeviceClipBounds.Size.Y);
            intermediate.DrawingSurface.Canvas.SetMatrix(fitMatrix);
            intermediate.DrawingSurface.Canvas.DrawSurface(surface, 0, 0);
            intermediate.DrawingSurface.Canvas.RestoreToCount(saved);

            target = intermediate.DrawingSurface;
        }

        OnPaint(context, target);

        if (useIntermediate)
        {
            surface.Canvas.DrawSurface(target, 0, 0);
        }
    }

    protected abstract void OnPaint(RenderContext context, DrawingSurface surface);

    public virtual RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return new RectD(0, 0, lastDocumentSize.X, lastDocumentSize.Y);
    }

    public virtual bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName)
    {
        OnPaint(context, renderOn);
        return true;
    }

    protected Texture RequestTexture(int id, VecI size, ColorSpace processingCs, bool clear = true)
    {
        return textureCache.RequestTexture(id, size, processingCs, clear);
    }

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        additionalData["AllowHighDpiRendering"] = AllowHighDpiRendering;
    }

    internal override void DeserializeAdditionalData(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalData(target, data, infos);

        if(data.TryGetValue("AllowHighDpiRendering", out var value))
            AllowHighDpiRendering = (bool)value;
    }

    public override void Dispose()
    {
        base.Dispose();
        textureCache.Dispose(); 
    }

   
}
