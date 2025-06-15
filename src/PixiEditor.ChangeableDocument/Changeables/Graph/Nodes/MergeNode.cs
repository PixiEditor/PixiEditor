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


    protected override void OnPaint(RenderContext context, DrawingSurface target)
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

    private void Merge(DrawingSurface target, RenderContext context)
    {
        if (Bottom.Value != null && Top.Value != null && BlendMode.Value != Enums.BlendMode.Normal)
        {
            int saved = target.Canvas.SaveLayer();
            Bottom.Value?.Paint(context, target);
            target.Canvas.RestoreToCount(saved);

            paint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            target.Canvas.SaveLayer(paint);

            Top.Value?.Paint(context, target);
            target.Canvas.RestoreToCount(saved);
            return;
        }

        Bottom.Value?.Paint(context, target);
        Top.Value?.Paint(context, target);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if (Top.Value == null && Bottom.Value == null)
        {
            return null;
        }

        RectD? totalBounds = null;

        if (Top.Connection != null && Top.Connection.Node is IPreviewRenderable topPreview)
        {
            var topBounds = topPreview.GetPreviewBounds(frame, elementToRenderName);
            if (topBounds != null)
            {
                totalBounds = totalBounds?.Union(topBounds.Value) ?? topBounds;
            }
        }

        if (Bottom.Connection != null && Bottom.Connection.Node is IPreviewRenderable bottomPreview)
        {
            var bottomBounds = bottomPreview.GetPreviewBounds(frame, elementToRenderName);
            if (bottomBounds != null)
            {
                totalBounds = totalBounds?.Union(bottomBounds.Value) ?? bottomBounds;
            }
        }

        return totalBounds;
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (Top.Value == null && Bottom.Value == null)
        {
            return false;
        }

        Merge(renderOn, context);

        return true;
    }

    public override void Dispose()
    {
        base.Dispose();
        paint.Dispose();
    }
}
