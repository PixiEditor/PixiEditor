using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("DocumentFilePath")]
public class DocumentFilePathNode : Node
{
    public InputProperty<DocumentReference> Document { get; }
    public OutputProperty<string> FilePath { get; }

    public DocumentFilePathNode()
    {
        Document = CreateInput<DocumentReference>("Document", "DOCUMENT", null);
        FilePath = CreateOutput<string>("FilePath", "FILE_PATH", string.Empty);
    }

    protected override void OnExecute(RenderContext context)
    {
        FilePath.Value = Document.Value?.OriginalFilePath ?? string.Empty;
    }

    public override Node CreateCopy() => new DocumentFilePathNode();
}
