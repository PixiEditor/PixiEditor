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
        /*Top = CreateRenderInput("Top", "TOP", context =>
        {
            var output = Output.GetFirstRenderTarget(context);
            if(output == null)
            {
                return null;
            }

            topLayer = output.Canvas.SaveLayer(blendPaint);
            return output;
        });
        Bottom = CreateRenderInput("Bottom", "BOTTOM", context =>
        {
            var output = Output.GetFirstRenderTarget(context);
            
            if(output == null)
            {
                return null;
            }
            
            bottomLayer = output.Canvas.SaveLayer(blendPaint);
            return output;
        });*/
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

        Merge(target);
    }

    private void Merge(DrawingSurface target)
    {
        if (Bottom.Value != null && Top.Value != null)
        {
            Texture texTop = RequestTexture(0, target.DeviceClipBounds.Size, false);
            Texture texBottom = RequestTexture(1, target.DeviceClipBounds.Size, false);
            
            paint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            texBottom.DrawingSurface.Canvas.DrawSurface(texTop.DrawingSurface, 0, 0, blendPaint);
            
            target.Canvas.DrawSurface(texTop.DrawingSurface, 0, 0);
            return;
        }
        
        if(Bottom.Value != null)
        {
            Texture tex = RequestTexture(1, target.DeviceClipBounds.Size, false);
            target.Canvas.DrawSurface(tex.DrawingSurface, 0, 0);
        }

        if(Top.Value != null)
        {
            Texture tex = RequestTexture(0, target.DeviceClipBounds.Size, false);
            target.Canvas.DrawSurface(tex.DrawingSurface, 0, 0);
        }
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

        Merge(renderOn);

        return true;
    }

    public override void Dispose()
    {
        base.Dispose();
        paint.Dispose();
    }
}
