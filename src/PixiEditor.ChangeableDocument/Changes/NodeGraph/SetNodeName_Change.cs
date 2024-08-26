using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class SetNodeName_Change : Change
{
    private Guid nodeId;
    private string newName;
    private string originalName;
    
    [GenerateMakeChangeAction]
    public SetNodeName_Change(Guid nodeId, string name)
    {
        this.nodeId = nodeId;
        this.newName = name;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        Node node = target.FindNode(nodeId);
        if (node == null)
        {
            return false;
        }

        originalName = node.DisplayName;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        Node node = target.FindNode(nodeId);
        node.DisplayName = newName;
        
        ignoreInUndo = false;
        return new NodeName_ChangeInfo(nodeId, newName);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node node = target.FindNode(nodeId);
        node.DisplayName = originalName;
        return new NodeName_ChangeInfo(nodeId, originalName);
    }
}
