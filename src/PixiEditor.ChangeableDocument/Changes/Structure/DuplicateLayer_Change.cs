using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class DuplicateLayer_Change : Change
{
    private readonly Guid layerGuid;
    private Guid duplicateGuid;

    private ConnectionsData? connectionsData;
    private Dictionary<Guid, VecD> originalPositions;

    [GenerateMakeChangeAction]
    public DuplicateLayer_Change(Guid layerGuid, Guid newGuid)
    {
        this.layerGuid = layerGuid;
        this.duplicateGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember<LayerNode>(layerGuid, out LayerNode? layer))
            return false;

        (_, Node parent) = ((LayerNode, Node))target.FindChildAndParent(layerGuid);

        connectionsData = NodeOperations.CreateConnectionsData(layer);

        if(parent == null)
        {
            FailedMessage = "GRAPH_STATE_UNABLE_TO_CREATE_MEMBER";
            return false;
        }

        var targetInput = parent.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter) &&
            x.Connection is { Node: StructureNode }) as InputProperty<Painter?>;

        if (targetInput == null)
        {
            FailedMessage = "GRAPH_STATE_UNABLE_TO_CREATE_MEMBER";
            return false;
        }

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        (LayerNode existingLayer, Node parent) = ((LayerNode, Node))target.FindChildAndParentOrThrow(layerGuid);


        LayerNode clone = (LayerNode)existingLayer.Clone();
        clone.Id = duplicateGuid;

        InputProperty<Painter?> targetInput = parent.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter) &&
            x.Connection is { Node: StructureNode }) as InputProperty<Painter?>;

        var previousConnection = targetInput?.Connection;

        List<IChangeInfo> operations = new();

        target.NodeGraph.AddNode(clone);

        operations.Add(CreateLayer_ChangeInfo.FromLayer(clone));

        operations.AddRange(NodeOperations.AppendMember(targetInput, clone.Output, clone.Background, clone.Id));

        operations.AddRange(NodeOperations.AdjustPositionsAfterAppend(clone, targetInput.Node,
            previousConnection?.Node as Node, out originalPositions));

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
            changes.AddRange(
                NodeOperations.ConnectStructureNodeProperties(connectionsData, originalNode, target.NodeGraph));
        }

        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));

        return changes;
    }
}
