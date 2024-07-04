namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class FolderNode : Node
{
    public InputProperty<ChunkyImage> Input { get; }
    public OutputProperty<ChunkyImage> Output { get; }
    
    public FolderNode(string name) : base(name)
    {
        Input = CreateInput<ChunkyImage>("Input", null);
        Output = CreateOutput<ChunkyImage>("Output", null);
    }    
    
    public override bool Validate()
    {
        return true;
    }
    
    public override void OnExecute(int frame)
    {
        Output.Value = Input.Value;
    }
}
