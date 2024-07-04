using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class LayerNode : Node
{
    public InputProperty<ChunkyImage?> Background { get; }
    public OutputProperty<ChunkyImage> Output { get; }
    public ChunkyImage LayerImage { get; set; }
    
    public LayerNode(string name, VecI size) : base(name)
    {
        Background = CreateInput<ChunkyImage>("Background", null);
        Output = CreateOutput<ChunkyImage>("Image", new ChunkyImage(size));
        LayerImage = new ChunkyImage(size);
    }

    public override bool Validate()
    {
        return true;
    }

    public override void OnExecute(int frame)
    {
        if (Background.Value != null)
        {
            Output.Value.EnqueueDrawChunkyImage(VecI.Zero, Background.Value);
        }
        
        Output.Value.EnqueueDrawChunkyImage(VecI.Zero, LayerImage);
        Output.Value.CommitChanges();
    }

   
}
