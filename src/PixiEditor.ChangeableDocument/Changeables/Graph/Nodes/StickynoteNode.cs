using PixiEditor.ChangeableDocument.Rendering;
namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("StickyNote")]
public class StickyNoteNode : Node
{
    public const string TitlePropertyName = "Title";
    public const string TextPropertyName = "Text";

    public InputProperty<string> Title { get; }
    public InputProperty<string> Text { get; }

    public StickyNoteNode()
    {
        Title = CreateInput(TitlePropertyName, "TITLE", "");
        Text = CreateInput(TextPropertyName, "TEXT_LABEL", "");
    }

    public override Node CreateCopy() => new StickyNoteNode();

    protected override void OnExecute(RenderContext context)
    {
        
    }
}
