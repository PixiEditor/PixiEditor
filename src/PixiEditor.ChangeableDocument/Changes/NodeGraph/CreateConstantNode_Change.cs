using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateConstantNode_Change : Change
{
    public Guid NodeId { get; }
    
    public Guid ConstantId { get; }
 
    [GenerateMakeChangeAction]
    public CreateConstantNode_Change(Guid nodeId, Guid constantId)
    {
        NodeId = nodeId;
        ConstantId = constantId;
    }

    public override bool InitializeAndValidate(Document target) => true;

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var constant = target.NodeGraph.Constants.First(x => x.Id == ConstantId);
        var node = new ConstantNode(constant) { Id = NodeId };

        target.NodeGraph.AddNode(node);
        
        ignoreInUndo = false;
        return CreateNode_ChangeInfo.CreateFromNode(node);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node node = target.FindNodeOrThrow<Node>(NodeId);
        target.NodeGraph.RemoveNode(node);

        return new DeleteNode_ChangeInfo(NodeId);
    }
}
