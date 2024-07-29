using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.DrawingApi.Core;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class DuplicateLayer_Change : Change
{
    private readonly Guid layerGuid;
    private Guid duplicateGuid;
    
    private ConnectionsData? connectionsData;

    [GenerateMakeChangeAction]
    public DuplicateLayer_Change(Guid layerGuid)
    {
        this.layerGuid = layerGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember<LayerNode>(layerGuid, out LayerNode? layer))
            return false;
        duplicateGuid = Guid.NewGuid();
        
        connectionsData = NodeOperations.CreateConnectionsData(layer);
        
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        (LayerNode existingLayer, Node parent) = ((LayerNode, Node))target.FindChildAndParentOrThrow(layerGuid);


        LayerNode clone = (LayerNode)existingLayer.Clone();
        clone.Id = duplicateGuid;

        InputProperty<Surface?> targetInput = parent.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Surface) &&
            x.Connection.Node is StructureNode) as InputProperty<Surface?>;

        List<IChangeInfo> operations = new();

        target.NodeGraph.AddNode(clone);

        operations.Add(CreateLayer_ChangeInfo.FromLayer(clone));

        operations.AddRange(NodeOperations.AppendMember(targetInput, clone.Output, clone.Background, clone.Id));

        ignoreInUndo = false;

        return operations;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var (member, parent) = target.FindChildAndParentOrThrow(duplicateGuid);

        target.NodeGraph.RemoveNode(member);
        member.Dispose();

        List<IChangeInfo> changes = new();

        changes.AddRange(NodeOperations.DetachStructureNode(member));
        changes.Add(new DeleteStructureMember_ChangeInfo(member.Id));
        
        if (connectionsData is not null)
        {
            Node originalNode = target.FindNodeOrThrow<Node>(layerGuid);
            changes.AddRange(NodeOperations.ConnectStructureNodeProperties(connectionsData, originalNode, target.NodeGraph));
        }
        
        return changes;
    }
}
