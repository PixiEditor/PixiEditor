using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class ImportLayer_Change : Change
{
    private ICrossDocumentPipe<IReadOnlyLayerNode> sourceDocumentPipe;
    private Dictionary<Guid, VecD> originalPositions;
    private ConnectionsData? connectionsData;

    private Guid duplicateGuid;

    [GenerateMakeChangeAction]
    public ImportLayer_Change(ICrossDocumentPipe<IReadOnlyLayerNode> pipe, Guid newGuid)
    {
        sourceDocumentPipe = pipe;
        duplicateGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (sourceDocumentPipe is not { CanOpen: true })
            return false;

        if (!sourceDocumentPipe.IsOpen)
        {
            sourceDocumentPipe.Open();
        }

        IReadOnlyLayerNode? layer = sourceDocumentPipe.TryAccessData();
        if (layer == null || target.NodeGraph.OutputNode == null)
            return false;

        connectionsData = NodeOperations.CreateConnectionsData(target.NodeGraph.OutputNode);

        if (target.NodeGraph.OutputNode == null) return false;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;

        var layer = sourceDocumentPipe.TryAccessData();
        if (layer is not LayerNode layerNode)
        {
            ignoreInUndo = true;
            return new None();
        }

        var clone = (LayerNode)layerNode.Clone();
        clone.Id = duplicateGuid;

        var targetInput = target.NodeGraph.OutputNode?.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter)) as InputProperty<Painter?>;

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
            Node originalNode = target.NodeGraph.OutputNode;
            changes.AddRange(
                NodeOperations.ConnectStructureNodeProperties(connectionsData, originalNode, target.NodeGraph));
        }

        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));

        return changes;
    }

    public override void Dispose()
    {
        sourceDocumentPipe?.Dispose();
    }
}

