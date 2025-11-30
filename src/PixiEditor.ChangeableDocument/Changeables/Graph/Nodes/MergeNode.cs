using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Merge")]
public class MergeNode : RenderNode
{
    public InputProperty<BlendMode> BlendMode { get; }
    public RenderInputProperty Top { get; }
    public RenderInputProperty Bottom { get; }

    private Paint paint = new Paint();

    private int topLayer;
    private int bottomLayer;

    public MergeNode()
    {
        BlendMode = CreateInput("BlendMode", "BlendMode", Enums.BlendMode.Normal);
        Top = CreateRenderInput("Top", "TOP");
        Bottom = CreateRenderInput("Bottom", "BOTTOM");
    }

    public override Node CreateCopy()
    {
        return new MergeNode();
    }


    protected override void OnPaint(RenderContext context, Canvas target)
    {
        if (Top.Value == null && Bottom.Value == null)
        {
            return;
        }

        if (target == null || target.DeviceClipBounds.Size == VecI.Zero)
        {
            return;
        }

        AllowHighDpiRendering = true;
        Merge(target, context);
    }

    private void Merge(Canvas target, RenderContext context)
    {
        if (Bottom.Value != null && Top.Value != null)
        {
            int saved = target.SaveLayer();
            Bottom.Value?.Paint(context, target);
            target.RestoreToCount(saved);

            paint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            target.SaveLayer(paint);

            Top.Value?.Paint(context, target);
            target.RestoreToCount(saved);
            return;
        }

        Bottom.Value?.Paint(context, target);
        Top.Value?.Paint(context, target);
    }

    protected override bool ShouldRenderPreview(string elementToRenderName)
    {
        return Top.Value != null || Bottom.Value != null;
    }

    public override void RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (Top.Value == null && Bottom.Value == null)
        {
            return;
        }

        Merge(renderOn.Canvas, context);
    }

    public override void Dispose()
    {
        base.Dispose();
        paint.Dispose();
    }
}
