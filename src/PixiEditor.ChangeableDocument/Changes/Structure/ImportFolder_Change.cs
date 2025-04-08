using System.Collections.Immutable;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class ImportFolder_Change : Change
{
    private ICrossDocumentPipe<IReadOnlyFolderNode> sourcefolderPipe;
    private Guid duplicateGuid;
    private Guid[] contentGuids;
    private Guid[] contentDuplicateGuids;

    private Guid[]? childGuidsToUse;

    private ConnectionsData? connectionsData;
    private Dictionary<Guid, ConnectionsData> contentConnectionsData = new();
    private Dictionary<Guid, VecD> originalPositions;

    [GenerateMakeChangeAction]
    public ImportFolder_Change(ICrossDocumentPipe<IReadOnlyFolderNode> pipe, Guid newGuid, ImmutableList<Guid>? childGuids)
    {
        sourcefolderPipe = pipe;
        duplicateGuid = newGuid;
        childGuidsToUse = childGuids?.ToArray();
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (sourcefolderPipe is not { CanOpen: true } || target.NodeGraph.OutputNode == null)
            return false;

        var folder = sourcefolderPipe.TryAccessData();

        connectionsData = NodeOperations.CreateConnectionsData(target.NodeGraph.OutputNode);

        List<Guid> contentGuidList = new();

        folder.Content.Connection?.Node.TraverseBackwards(x =>
        {
            contentGuidList.Add(x.Id);
            contentConnectionsData[x.Id] = NodeOperations.CreateConnectionsData(x);
            return true;
        });

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var readOnlyFolderNode = sourcefolderPipe.TryAccessData();

        if (readOnlyFolderNode is not FolderNode folderNode || target.NodeGraph.OutputNode == null)
        {
            ignoreInUndo = true;
            return new None();
        }

        FolderNode clone = (FolderNode)folderNode.Clone();
        clone.Id = duplicateGuid;

        InputProperty<Painter?> targetInput = target.NodeGraph.OutputNode.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter)) as InputProperty<Painter?>;

        List<IChangeInfo> operations = new();

        target.NodeGraph.AddNode(clone);

        var previousConnection = targetInput.Connection;

        operations.Add(CreateNode_ChangeInfo.CreateFromNode(clone));
        operations.AddRange(NodeOperations.AppendMember(targetInput, clone.Output, clone.Background, clone.Id));
        operations.AddRange(NodeOperations.AdjustPositionsAfterAppend(clone, targetInput.Node,
            previousConnection?.Node as Node, out originalPositions));

        DuplicateContent(target, clone, folderNode, operations);

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

        if (contentDuplicateGuids is not null)
        {
            foreach (Guid contentGuid in contentDuplicateGuids)
            {
                Node contentNode = target.FindNodeOrThrow<Node>(contentGuid);
                changes.AddRange(NodeOperations.DetachNode(target.NodeGraph, contentNode));
                changes.Add(new DeleteNode_ChangeInfo(contentNode.Id));

                target.NodeGraph.RemoveNode(contentNode);
                contentNode.Dispose();
            }
        }

        if (connectionsData is not null)
        {
            Node originalNode = target.NodeGraph.OutputNode;
            changes.AddRange(
                NodeOperations.ConnectStructureNodeProperties(connectionsData, originalNode, target.NodeGraph));
        }

        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));

        return changes;
    }

    private void DuplicateContent(Document target, FolderNode clone, FolderNode existingLayer,
        List<IChangeInfo> operations)
    {
        Dictionary<Guid, Guid> nodeMap = new Dictionary<Guid, Guid>();

        nodeMap[existingLayer.Id] = clone.Id;
        int counter = 0;
        List<Guid> contentGuidList = new();

        existingLayer.Content.Connection?.Node.TraverseBackwards(x =>
        {
            if (x is not Node targetNode)
                return false;

            Node? node = targetNode.Clone();

            if (node is not FolderNode && childGuidsToUse is not null && counter < childGuidsToUse.Length)
            {
                node.Id = childGuidsToUse[counter];
                counter++;
            }

            nodeMap[x.Id] = node.Id;
            contentGuidList.Add(node.Id);

            target.NodeGraph.AddNode(node);

            operations.Add(CreateNode_ChangeInfo.CreateFromNode(node));
            return true;
        });

        foreach (var data in contentConnectionsData)
        {
            var updatedData = data.Value.WithUpdatedIds(nodeMap);
            Guid targetNodeId = nodeMap[data.Key];
            operations.AddRange(NodeOperations.ConnectStructureNodeProperties(updatedData,
                target.FindNodeOrThrow<Node>(targetNodeId), target.NodeGraph));
        }

        contentDuplicateGuids = contentGuidList.ToArray();
    }
}
