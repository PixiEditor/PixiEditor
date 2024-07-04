using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class MergeNode : Node
{
    public InputProperty<ChunkyImage?> Top { get; }
    public InputProperty<ChunkyImage?> Bottom { get; }
    public OutputProperty<ChunkyImage> Output { get; }
    
    public MergeNode(string name) : base(name)
    {
        Top = CreateInput<ChunkyImage>("Top", null);
        Bottom = CreateInput<ChunkyImage>("Bottom", null);
        Output = CreateOutput<ChunkyImage>("Output", null);
    }
    
    public override bool Validate()
    {
        return Top.Value != null || Bottom.Value != null;
    }

    public override void OnExecute(int frame)
    {
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
    }
}
