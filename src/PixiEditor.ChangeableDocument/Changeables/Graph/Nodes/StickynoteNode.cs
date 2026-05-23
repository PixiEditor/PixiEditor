using PixiEditor.ChangeableDocument.Rendering;
namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("StickyNote")]
public class StickyNoteNode : Node
{
    public InputProperty<string> Text { get; }

    public StickyNoteNode()
    {
        Text = CreateInput("Text", "TEXT_LABEL", "");
    }
    public override Node CreateCopy() => new StickyNoteNode();

    protected override void OnExecute(RenderContext context)
    {
        
    }
}
