using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class RenderNode : Node, IPreviewRenderable, IHighDpiRenderNode
{
    public RenderOutputProperty Output { get; }

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
    }

    protected abstract void OnPaint(RenderContext context, DrawingSurface surface);

    public abstract RectD? GetPreviewBounds(int frame, string elementToRenderName = "");

    public abstract bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName);

    protected Texture RequestTexture(int id, VecI size, ColorSpace processingCs, bool clear = true)
    {
        return textureCache.RequestTexture(id, size, processingCs, clear);
    }

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        additionalData["AllowHighDpiRendering"] = AllowHighDpiRendering;
    }

    internal override OneOf<None, IChangeInfo, List<IChangeInfo>> DeserializeAdditionalData(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data)
    {
        base.DeserializeAdditionalData(target, data);

        if(data.TryGetValue("AllowHighDpiRendering", out var value))
            AllowHighDpiRendering = (bool)value;

        return new None();
    }

    public override void Dispose()
    {
        base.Dispose();
        textureCache.Dispose(); 
    }

   
}
