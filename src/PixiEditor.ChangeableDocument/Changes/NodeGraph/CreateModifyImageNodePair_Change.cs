using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateModifyImageNodePair_Change : Change
{
    private Guid startId;
    private Guid endId;
    private Guid zoneId;
    
    [GenerateMakeChangeAction]
    public CreateModifyImageNodePair_Change(Guid startId, Guid endId, Guid zoneId)
    {
        this.startId = startId;
        this.endId = endId;
        this.zoneId = zoneId;
    }

    public override bool InitializeAndValidate(Document target) => true;

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var start = new ModifyImageLeftNode();
        var end = new ModifyImageRightNode(start);

        start.Id = startId;
        end.Id = endId;
        end.Position = new VecD(100, 0);
        
        target.NodeGraph.AddNode(start);
        target.NodeGraph.AddNode(end);
        
        ignoreInUndo = false;

        return new List<IChangeInfo>
        {
            CreateNode_ChangeInfo.CreateFromNode(start, "Modify Image"),
            CreateNode_ChangeInfo.CreateFromNode(end, "Modify Image"),
            new CreateNodeZone_ChangeInfo(zoneId, "PixiEditor.ModifyImageZone", startId, endId)
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var startChange = RemoveNode(target, startId);
        var endChange = RemoveNode(target, endId);
        var zoneChange = new DeleteNodeFrame_ChangeInfo(zoneId);

        return new List<IChangeInfo> { startChange, endChange, zoneChange };
    }

    private static DeleteNode_ChangeInfo RemoveNode(Document target, Guid id)
    {
        Node node = target.FindNodeOrThrow<Node>(id);
        target.NodeGraph.RemoveNode(node);

        return new DeleteNode_ChangeInfo(id);
    }
}
