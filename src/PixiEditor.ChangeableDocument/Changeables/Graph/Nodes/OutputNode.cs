using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Output")]
public class OutputNode : Node, IBackgroundInput
{
    public const string InputPropertyName = "Background";

    public override string DisplayName { get; set; } = "OUTPUT_NODE";
    public InputProperty<Texture?> Input { get; } 
    public OutputNode()
    {
        Input = CreateInput<Texture>(InputPropertyName, "INPUT", null);
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return Input.Value;
    }

    InputProperty<Texture?> IBackgroundInput.Background => Input;
}
