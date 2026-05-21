using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Stickynote")]
public class StickynoteNode : Node
{
    public InputProperty<string> Text { get; }

    public StickynoteNode()
    {
        Text = CreateInput("Text", "TEXT_LABEL", "");
    }
    public override Node CreateCopy() => new StickynoteNode();

    protected override void OnExecute(RenderContext context)
    {
        Console.WriteLine("hi");
    }
}