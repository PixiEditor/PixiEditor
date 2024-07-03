using SkiaSharp;

namespace Nodes.Nodes;

public class OutputNode : Node
{
    public InputProperty<SKSurface?> Input { get; } 
    public OutputNode(string name) : base(name)
    {
        Input = CreateInput<SKSurface>("Input", null);
    }
    
    public override bool Validate()
    {
        return Input.Value != null;
    }
    
    public override void OnExecute(int frame)
    {
        
    }
}
