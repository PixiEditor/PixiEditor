using SkiaSharp;

namespace Nodes.Nodes;

public class FolderNode : Node
{
    public InputProperty<SKSurface> Input { get; }
    public OutputProperty<SKSurface> Output { get; }
    
    public FolderNode(string name) : base(name)
    {
        Input = CreateInput<SKSurface>("Input", null);
        Output = CreateOutput<SKSurface>("Output", null);
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
