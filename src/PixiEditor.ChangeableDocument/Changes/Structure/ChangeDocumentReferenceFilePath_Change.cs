namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class ChangeDocumentReferenceFilePath_Change : Change
{
    private Guid id;
    private string? originalFilePath = null;
    private string newFilePath;

    [GenerateMakeChangeAction]
    public ChangeDocumentReferenceFilePath_Change(Guid id, string filePath)
    {
        this.id = id;
        this.newFilePath = filePath;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var node = target.FindNodeOrThrow<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>(id);
        return node.NestedDocument.NonOverridenValue?.DocumentInstance != null;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var node = target.FindNodeOrThrow<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>(id);
        originalFilePath = node.NestedDocument.NonOverridenValue?.OriginalFilePath;

        node.NestedDocument.NonOverridenValue.OriginalFilePath = newFilePath;


        ignoreInUndo = originalFilePath == newFilePath;
        return new ChangeInfos.Structure.NestedDocumentLink_ChangeInfo(id, newFilePath, node.NestedDocument.NonOverridenValue.ReferenceId);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNodeOrThrow<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>(id);
        node.NestedDocument.NonOverridenValue.OriginalFilePath = originalFilePath;

        return new ChangeInfos.Structure.NestedDocumentLink_ChangeInfo(id, originalFilePath, node.NestedDocument.NonOverridenValue.ReferenceId);
    }
}
