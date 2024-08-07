using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Output", "OUTPUT_NODE")]
public class OutputNode : Node, IBackgroundInput
{
    public const string InputPropertyName = "Background";

    public InputProperty<Surface?> Input { get; } 
    public OutputNode()
    {
        Input = CreateInput<Surface>(InputPropertyName, "INPUT", null);
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        return Input.Value;
    }

    InputProperty<Surface?> IBackgroundInput.Background => Input;
}
