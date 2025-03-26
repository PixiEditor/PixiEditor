using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class DeserializeNodeAdditionalData_Change : Change
{
    private Guid nodeId;
    private Dictionary<string, object> data;
    
    [GenerateMakeChangeAction]
    public DeserializeNodeAdditionalData_Change(Guid nodeId, Dictionary<string, object> data)
    {
        this.nodeId = nodeId;
        this.data = data;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindNode<Node>(nodeId, out _);   
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        Node node = target.FindNode<Node>(nodeId);

        List<IChangeInfo> infos = new();
        node.DeserializeAdditionalData(target, data, infos);
        ignoreInUndo = false;
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        throw new InvalidOperationException("Cannot revert UpdateNodeAdditionalData_Change, this change is only meant for deserialization of nodes.");
        return new None(); // do not remove, code generator doesn't work without it
    }
}
