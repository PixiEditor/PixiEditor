namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateNodeFromName_Change : Change
{
    private string nodeUniqueName;
    private Guid id;
    private Guid pairId;

    private Type typeToCreate;


    private CreateNode_Change change;

    [GenerateMakeChangeAction]
    public CreateNodeFromName_Change(string nodeUniqueName, Guid id, Guid pairId)
    {
        this.id = id;
        this.nodeUniqueName = nodeUniqueName;
        this.pairId = pairId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        bool isValidName = NodeOperations.TryGetNodeType(nodeUniqueName, out Type nodeType);
        if (!isValidName)
        {
            return false;
        }

        typeToCreate = nodeType;
        change = new CreateNode_Change(nodeType, id, pairId);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        return change.Apply(target, firstApply, out ignoreInUndo);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return change.Revert(target); 
    }

    public override void Dispose()
    {
        change?.Dispose();
    }
}
