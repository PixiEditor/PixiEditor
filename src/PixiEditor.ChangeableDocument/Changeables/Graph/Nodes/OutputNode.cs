using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class OutputNode : Node, IBackgroundInput
{
    public override string DisplayName { get; set; } = "OUTPUT_NODE";
    public InputProperty<Surface?> Input { get; } 
    public OutputNode()
    {
        Input = CreateInput<Surface>("Background", "INPUT", null);
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    protected override string NodeUniqueName => "Output";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return Input.Value;
    }

    InputProperty<Surface?> IBackgroundInput.Background => Input;
}
