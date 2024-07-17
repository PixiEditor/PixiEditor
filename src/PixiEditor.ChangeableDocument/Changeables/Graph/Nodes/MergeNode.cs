using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class MergeNode : Node, IBackgroundInput
{
    public InputProperty<Chunk?> Top { get; }
    public InputProperty<Chunk?> Bottom { get; }
    public OutputProperty<Chunk?> Output { get; }
    
    public MergeNode() 
    {
        Top = CreateInput<Chunk?>("Top", "TOP", null);
        Bottom = CreateInput<Chunk?>("Bottom", "BOTTOM", null);
        Output = CreateOutput<Chunk?>("Output", "OUTPUT", null);
    }
    
    public override bool Validate()
    {
        return Top.Connection != null || Bottom.Connection != null;
    }

    public override Node CreateCopy()
    {
        return new MergeNode();
    }

    protected override Chunk? OnExecute(RenderingContext context)
    {
        /*if(Top.Value == null && Bottom.Value == null)
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
            workingSurface.DrawingSurface.Canvas.DrawSurface(Top.Value.DrawingSurface, 0, 0);
        }

        Output.Value = workingSurface;
        
        return Output.Value;*/
        
        return null;
    }

    InputProperty<Chunk> IBackgroundInput.Background => Bottom;
}
