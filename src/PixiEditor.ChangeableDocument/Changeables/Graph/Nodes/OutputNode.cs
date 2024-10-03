using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Output")]
public class OutputNode : Node, IBackgroundInput
{
    public const string InputPropertyName = "Background";

    public InputProperty<DrawingSurface?> Input { get; } 
    public OutputNode()
    {
        Input = CreateInput<DrawingSurface>(InputPropertyName, "INPUT", null);
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    InputProperty<DrawingSurface?> IBackgroundInput.Background => Input;
}
