namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class OutputNode : Node
{
    public InputProperty<ChunkyImage?> Input { get; } 
    public OutputNode(string name) : base(name)
    {
        Input = CreateInput<ChunkyImage>("Input", null);
    }
    
    public override bool Validate()
    {
        return Input.Value != null;
    }
    
    public override void OnExecute(int frame)
    {
        
    }
}
