using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class OutputNode : Node, IBackgroundInput
{
    public InputProperty<Image?> Input { get; } 
    public OutputNode()
    {
        Input = CreateInput<Image>("Background", "INPUT", null);
    }
    
    public override bool Validate()
    {
        return Input.Connection != null;
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    protected override Image? OnExecute(KeyFrameTime frame)
    {
        return Input.Value;
    }

    InputProperty<Image?> IBackgroundInput.Background => Input;
}
