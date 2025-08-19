using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ApplyFilter")]
public class ApplyFilterNode : RenderNode, IRenderInput
{
    private Paint _paint = new();
    public InputProperty<Filter?> Filter { get; }

    public RenderInputProperty Background { get; }

    public ApplyFilterNode()
    {
        Background = CreateRenderInput("Input", "IMAGE");
        Filter = CreateInput<Filter>("Filter", "FILTER", null);
        Output.FirstInChain = null;
        AllowHighDpiRendering = true;
    }


    protected override void Paint(RenderContext context, DrawingSurface surface)
    {
        AllowHighDpiRendering = (Background.Connection.Node as RenderNode)?.AllowHighDpiRendering ?? true;
        base.Paint(context, surface);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (_paint == null)
            return;

        _paint.SetFilters(Filter.Value);

        if (!context.ProcessingColorSpace.IsSrgb)
        {
            using var intermediate = Texture.ForProcessing(surface, context.ProcessingColorSpace);

            int saved = surface.Canvas.Save();
            surface.Canvas.SetMatrix(Matrix3X3.Identity);

            Background.Value?.Paint(context, intermediate.DrawingSurface);

            using var srgbSurface = Texture.ForProcessing(intermediate.Size, ColorSpace.CreateSrgb());

            srgbSurface.DrawingSurface.Canvas.SaveLayer(_paint);
            srgbSurface.DrawingSurface.Canvas.DrawSurface(intermediate.DrawingSurface, 0, 0);
            srgbSurface.DrawingSurface.Canvas.Restore();

            surface.Canvas.DrawSurface(srgbSurface.DrawingSurface, 0, 0);
            surface.Canvas.RestoreToCount(saved);
        }
        else
        {
            int layer = surface.Canvas.SaveLayer(_paint);
            Background.Value?.Paint(context, surface);
            surface.Canvas.RestoreToCount(layer);
        }
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return PreviewUtils.FindPreviewBounds(Background.Connection, frame, elementToRenderName);
    }

    public override Node CreateCopy() => new ApplyFilterNode();
}
