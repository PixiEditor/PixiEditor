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
    public InputProperty<Surface?> Top { get; }
    public InputProperty<Surface?> Bottom { get; }
    public OutputProperty<Surface?> Output { get; }
    
    public MergeNode() 
    {
        BlendMode = CreateInput("BlendMode", "BlendMode", Enums.BlendMode.Normal);
        Top = CreateInput<Surface?>("Top", "TOP", null);
        Bottom = CreateInput<Surface?>("Bottom", "BOTTOM", null);
        Output = CreateOutput<Surface?>("Output", "OUTPUT", null);
    }

    public override Node CreateCopy()
    {
        return new MergeNode();
    }


    protected override Surface? OnExecute(RenderingContext context)
    {
        if(Top.Value == null && Bottom.Value == null)
        {
            Output.Value = null;
            return null;
        }
        
        int width = Top.Value?.Size.X ?? Bottom.Value.Size.X;
        int height = Top.Value?.Size.Y ?? Bottom.Value.Size.Y;
        
        Surface workingSurface = new Surface(new VecI(width, height));
        
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

    InputProperty<Surface> IBackgroundInput.Background => Bottom;
}
