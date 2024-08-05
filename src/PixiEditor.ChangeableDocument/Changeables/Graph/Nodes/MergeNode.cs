using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Merge")]
public class MergeNode : Node, IBackgroundInput
{
    public InputProperty<Texture?> Top { get; }
    public InputProperty<Texture?> Bottom { get; }
    public OutputProperty<Texture?> Output { get; }
    
    public MergeNode() 
    {
        Top = CreateInput<Texture?>("Top", "TOP", null);
        Bottom = CreateInput<Texture?>("Bottom", "BOTTOM", null);
        Output = CreateOutput<Texture?>("Output", "OUTPUT", null);
    }

    public override string DisplayName { get; set; } = "MERGE_NODE";

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
        
        int width = Top.Value?.Size.X ?? Bottom.Value.Size.X;
        int height = Top.Value?.Size.Y ?? Bottom.Value.Size.Y;
        
        Texture workingSurface = new Texture(new VecI(width, height));
        
        if(Bottom.Value != null)
        {
            workingSurface.Surface.Canvas.DrawSurface(Bottom.Value.Surface, 0, 0);
        }
        
        if(Top.Value != null)
        {
            workingSurface.Surface.Canvas.DrawSurface(Top.Value.Surface, 0, 0);
        }

        Output.Value = workingSurface;
        
        return Output.Value;
    }

    InputProperty<Texture> IBackgroundInput.Background => Bottom;
}
