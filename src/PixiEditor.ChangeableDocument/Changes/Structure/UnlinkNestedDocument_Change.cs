namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class UnlinkNestedDocument_Change : Change
{
    private Guid id;
    private string? originalFilePath = null;
    private Guid originalReferenceId = Guid.Empty;
    private Guid newUniqueId = Guid.NewGuid();

    [GenerateMakeChangeAction]
    public UnlinkNestedDocument_Change(Guid id)
    {
        this.id = id;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var node = target.FindNodeOrThrow<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>(id);
        return node.NestedDocument.NonOverridenValue?.OriginalFilePath != null;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var node = target.FindNodeOrThrow<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>(id);
        originalFilePath = node.NestedDocument.NonOverridenValue?.OriginalFilePath;
        originalReferenceId = node.NestedDocument.NonOverridenValue?.ReferenceId ?? Guid.Empty;

        node.NestedDocument.NonOverridenValue.OriginalFilePath = null;
        // Reference ID should always be present after unlinking, since duplicating, editing in external tab should still work.
        node.NestedDocument.NonOverridenValue.ReferenceId = newUniqueId;


        ignoreInUndo = false;
        return new ChangeInfos.Structure.NestedDocumentLink_ChangeInfo(id, null, node.NestedDocument.NonOverridenValue.ReferenceId);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNodeOrThrow<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>(id);
        node.NestedDocument.NonOverridenValue.OriginalFilePath = originalFilePath;
        node.NestedDocument.NonOverridenValue.ReferenceId = originalReferenceId;

        return new ChangeInfos.Structure.NestedDocumentLink_ChangeInfo(id, originalFilePath, originalReferenceId);
    }
}
