using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class OutputNode : Node, IBackgroundInput
{
    public InputProperty<ChunkyImage?> Input { get; } 
    public OutputNode()
    {
        Input = CreateInput<ChunkyImage>("Background", "INPUT", null);
    }
    
    public override bool Validate()
    {
        return Input.Value != null;
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    public override ChunkyImage? OnExecute(KeyFrameTime frame)
    {
        return Input.Value;
    }

    InputProperty<ChunkyImage?> IBackgroundInput.Background => Input;
}
