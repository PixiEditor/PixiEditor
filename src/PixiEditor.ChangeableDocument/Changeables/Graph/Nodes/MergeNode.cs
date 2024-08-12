using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Merge", "MERGE_NODE")]
public class MergeNode : Node, IBackgroundInput
{
    private Paint _paint = new();

    public InputProperty<BlendMode> BlendMode { get; }
    public InputProperty<Texture?> Top { get; }
    public InputProperty<Texture?> Bottom { get; }
    public OutputProperty<Texture?> Output { get; }
    
    public MergeNode() 
    {
        BlendMode = CreateInput("BlendMode", "BlendMode", Enums.BlendMode.Normal);
        Top = CreateInput<Texture?>("Top", "TOP", null);
        Bottom = CreateInput<Texture?>("Bottom", "BOTTOM", null);
        Output = CreateOutput<Texture?>("Output", "OUTPUT", null);
    }

    public override Node CreateCopy()
    {
        return new MergeNode();
    }


    protected override Texture? OnExecute(RenderingContext context)
    {
        if(Top.Value == null && Bottom.Value == null)
        {
            Output.Value = null;
            return null;
        }
        
        int width = Math.Max(Top.Value?.Size.X ?? Bottom.Value.Size.X, Bottom.Value.Size.X);
        int height = Math.Max(Top.Value?.Size.Y ?? Bottom.Value.Size.Y, Bottom.Value.Size.Y);
        
        Texture workingSurface = RequestTexture(0, new VecI(width, height), true);
        
        if(Bottom.Value != null)
        {
            workingSurface.DrawingSurface.Canvas.DrawSurface(Bottom.Value.DrawingSurface, 0, 0);
        }

        if(Top.Value != null)
        {
            _paint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            workingSurface.DrawingSurface.Canvas.DrawSurface(Top.Value.DrawingSurface, 0, 0, _paint);
        }

        Output.Value = workingSurface;
        
        return Output.Value;
    }

    InputProperty<Texture> IBackgroundInput.Background => Bottom;
}
