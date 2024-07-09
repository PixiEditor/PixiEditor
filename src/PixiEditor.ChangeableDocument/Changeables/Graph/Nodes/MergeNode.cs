using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class MergeNode : Node
{
    public InputProperty<ChunkyImage?> Top { get; }
    public InputProperty<ChunkyImage?> Bottom { get; }
    public OutputProperty<ChunkyImage> Output { get; }
    
    public MergeNode() 
    {
        Top = CreateInput<ChunkyImage>("Top", "TOP", null);
        Bottom = CreateInput<ChunkyImage>("Bottom", "BOTTOM", null);
        Output = CreateOutput<ChunkyImage>("Output", "OUTPUT", null);
    }
    
    public override bool Validate()
    {
        return Top.Value != null || Bottom.Value != null;
    }

    public override ChunkyImage? OnExecute(KeyFrameTime frame)
    {
        if(Top.Value == null && Bottom.Value == null)
        {
            return null;
        }
        
        VecI size = Top.Value?.CommittedSize ?? Bottom.Value.CommittedSize;
        
        Output.Value = new ChunkyImage(size);
        
        if (Bottom.Value != null)
        {
            Output.Value.EnqueueDrawChunkyImage(VecI.Zero, Bottom.Value);
        }
        
        if (Top.Value != null)
        {
            Output.Value.EnqueueDrawChunkyImage(VecI.Zero, Top.Value);
        }
        
        Output.Value.CommitChanges();
        
        return Output.Value;
    }
}
