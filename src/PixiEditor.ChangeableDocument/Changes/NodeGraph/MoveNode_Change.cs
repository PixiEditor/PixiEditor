using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Arrays;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class MoveNode_Change : Change
{
    private readonly Guid nodeToMove;
    private readonly Guid targetNodeGuid;

    private Dictionary<Guid, VecD> originalPositions = new();

    private ConnectionsData originalConnections;
    private ConnectionsData originalConnectionsOfTarget;

    [GenerateMakeChangeAction]
    public MoveNode_Change(Guid nodeToMove, Guid targetNodeGuid)
    {
        this.nodeToMove = nodeToMove;
        this.targetNodeGuid = targetNodeGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        Node node = target.FindNode(nodeToMove);
        Node targetNode = target.FindNode(targetNodeGuid);

        if (node == null || targetNode == null || node == targetNode)
            return false;

        originalConnections = NodeOperations.CreateConnectionsData(node);
        originalConnectionsOfTarget = NodeOperations.CreateConnectionsData(targetNode);

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        List<IChangeInfo> changes = new();

        Node nodeBeingMoved = target.FindNode(nodeToMove) as Node;
        Node dropOntoNode = target.FindNode(targetNodeGuid) as Node;

        VecD sourceOriginalPosition = nodeBeingMoved.Position;

        Node? oldInputConnectionNode = nodeBeingMoved.InputProperties.FirstOrDefault()?.Connection?.Node as Node;
        Node? oldOutputConnectionNode =
            nodeBeingMoved.OutputProperties.FirstOrDefault()?.Connections.FirstOrDefault()?.Node as Node;
        bool hadMultipleOutputs = nodeBeingMoved.OutputProperties.FirstOrDefault()?.Connections.Count > 1;

        ignoreInUndo = false;
        changes.AddRange(NodeOperations.DetachNodeWithAutoconnect(nodeBeingMoved));
        var outputToConnect = nodeBeingMoved.OutputProperties.FirstOrDefault();
        var toConnectTo = outputToConnect != null
            ? dropOntoNode.InputProperties.FirstOrDefault(x => outputToConnect.ValueType.IsAssignableTo(x.ValueType))
            : null;
        var nodeInput = nodeBeingMoved.InputProperties.FirstOrDefault();

        changes.AddRange(NodeOperations.AppendMember(toConnectTo, outputToConnect, nodeInput, nodeBeingMoved.Id));

        if (!hadMultipleOutputs && oldInputConnectionNode is not null && oldOutputConnectionNode is not null)
        {
            changes.AddRange(NodeOperations.CollapseFreeSpaceAfterRemovingNode(oldInputConnectionNode,
                oldOutputConnectionNode, out var tempOriginalPositions));
            originalPositions.AddRangeNewOnly(tempOriginalPositions);
        }

        Node? nodeBeforeInserted = nodeBeingMoved.InputProperties.FirstOrDefault()?.Connection?.Node as Node;
        changes.AddRange(NodeOperations.PushNodesBackAfterInsertingNodeBetweenTwoOthers(nodeBeingMoved, dropOntoNode,
            nodeBeforeInserted, out var tempOriginalPositions2));
        originalPositions.AddRangeNewOnly(tempOriginalPositions2);
        originalPositions[nodeBeingMoved.Id] = sourceOriginalPosition;

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new();

        Node node = target.FindNode(nodeToMove) as Node;

        if (node == null || originalConnections == null)
            return changes;


        changes.AddRange(NodeOperations.DetachNodeWithAutoconnect(node));
        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(originalConnections, node, target.NodeGraph));
        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));
        return changes;
    }
}
