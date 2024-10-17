using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Merge")]
public class MergeNode : RenderNode
{
    public InputProperty<BlendMode> BlendMode { get; }
    public RenderInputProperty Top { get; }
    public RenderInputProperty Bottom { get; }

    private Paint paint = new Paint();
    
    private static readonly Paint blendPaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.SrcOver };

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
        if(Top.Value == null && Bottom.Value == null)
        {
            return;
        }
        
        if(target == null || target.DeviceClipBounds.Size == VecI.Zero)
        {
            return;
        }

        Merge(target, context);
    }

    private void Merge(DrawingSurface target, RenderContext context)
    {
        if (Bottom.Value != null && Top.Value != null)
        {
            int saved = target.Canvas.SaveLayer();
            Bottom.Value.Paint(context, target);

            paint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            target.Canvas.SaveLayer(paint);
            
            Top.Value.Paint(context, target);
            target.Canvas.RestoreToCount(saved);
            return;
        }

        Bottom.Value?.Paint(context, target);
        Top.Value?.Paint(context, target);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if(Top.Value == null && Bottom.Value == null)
        {
            return null;
        }
        
        return new RectD(VecI.Zero, new VecI(128, 128)); 
    }

    public override bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        if (Top.Value == null && Bottom.Value == null)
        {
            return false;
        }

        using RenderContext context = new RenderContext(renderOn, frame, ChunkResolution.Full, VecI.Zero);
        Merge(renderOn, context);

        return true;
    }

    public override void Dispose()
    {
        base.Dispose();
        paint.Dispose();
    }
}
