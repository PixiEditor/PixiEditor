using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateNodeFrame_Change : Change
{
    private Guid id;
    private IEnumerable<Guid> nodeIds;
    
    [GenerateMakeChangeAction]
    public CreateNodeFrame_Change(Guid id, IEnumerable<Guid> nodeIds)
    {
        this.id = id;
        this.nodeIds = nodeIds;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        return new CreateNodeFrame_ChangeInfo(id, nodeIds);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return new DeleteNodeFrame_ChangeInfo(id);
    }
}
