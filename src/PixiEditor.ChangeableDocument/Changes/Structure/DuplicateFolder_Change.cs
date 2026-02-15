using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class DuplicateFolder_Change : Change
{
    private readonly Guid folderGuid;
    private Guid duplicateGuid;
    private Guid[] contentGuids;
    private Guid[] contentDuplicateGuids;

    private Guid[]? childGuidsToUse;
    private Dictionary<Guid, List<Guid>> keyFramesMap = new();
    private Dictionary<Guid, Guid> nodeMap = new();

    private ConnectionsData? connectionsData;
    private Dictionary<Guid, ConnectionsData> contentConnectionsData = new();
    private Dictionary<Guid, VecD> originalPositions;

    [GenerateMakeChangeAction]
    public DuplicateFolder_Change(Guid folderGuid, Guid newGuid, ImmutableList<Guid>? childGuids)
    {
        this.folderGuid = folderGuid;
        duplicateGuid = newGuid;
        childGuidsToUse = childGuids?.ToArray();
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember<FolderNode>(folderGuid, out FolderNode? folder))
            return false;

        connectionsData = NodeOperations.CreateConnectionsData(folder);

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
        (FolderNode existingLayer, Node parent) = ((FolderNode, Node))target.FindChildAndParentOrThrow(folderGuid);

        FolderNode clone = (FolderNode)existingLayer.Clone();
        clone.Id = duplicateGuid;

        InputProperty<Painter?> targetInput = parent.InputProperties.FirstOrDefault(x =>
            x.ValueType == typeof(Painter) &&
            x.Connection is { Node: StructureNode }) as InputProperty<Painter?>;

        List<IChangeInfo> operations = new();

        target.NodeGraph.AddNode(clone);

        var previousConnection = targetInput.Connection;

        operations.Add(CreateNode_ChangeInfo.CreateFromNode(clone));
        operations.AddRange(NodeOperations.AppendMember(targetInput, clone.Output, clone.Background, clone.Id));
        operations.AddRange(NodeOperations.AdjustPositionsAfterAppend(clone, targetInput.Node,
            previousConnection?.Node as Node, out originalPositions));

        DuplicateContent(target, clone, existingLayer, operations, firstApply);

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
                changes.AddRange(NodeOperations.DetachNode(contentNode));
                changes.Add(new DeleteNode_ChangeInfo(contentNode.Id));

                target.NodeGraph.RemoveNode(contentNode);
                contentNode.Dispose();
            }
        }

        if (connectionsData is not null)
        {
            Node originalNode = target.FindNodeOrThrow<Node>(folderGuid);
            changes.AddRange(
                NodeOperations.ConnectStructureNodeProperties(connectionsData, originalNode, target.NodeGraph));
        }

        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));

        return changes;
    }

    private void DuplicateContent(Document target, FolderNode clone, FolderNode existingLayer,
        List<IChangeInfo> operations, bool firstApply)
    {
        if (firstApply)
        {
            nodeMap = new Dictionary<Guid, Guid>();
            nodeMap[existingLayer.Id] = clone.Id;
        }

        int counter = 0;
        List<Guid> contentGuidList = new();

        if (firstApply)
        {
            keyFramesMap = new Dictionary<Guid, List<Guid>>();
        }

        int childCounter = 0;

        existingLayer.Content.Connection?.Node.TraverseBackwards(x =>
        {
            if (x is not Node targetNode)
                return false;

            Node? node = targetNode.Clone();

            if (contentDuplicateGuids != null && contentDuplicateGuids.Length > 0)
            {
                node.Id = contentDuplicateGuids[childCounter];
                childCounter++;
            }
            else
            {
                if (node is not FolderNode && childGuidsToUse is not null && counter < childGuidsToUse.Length)
                {
                    node.Id = childGuidsToUse[counter];
                    counter++;
                }
            }

            if (firstApply)
            {
                keyFramesMap[node.Id] = new List<Guid>();
                keyFramesMap[node.Id].AddRange(x.KeyFrames.Select(kf => kf.KeyFrameGuid));
            }
            else
            {
                if (keyFramesMap.TryGetValue(node.Id, out List<Guid>? keyFrameGuids))
                {
                    for (int i = 0; i < x.KeyFrames.Count; i++)
                    {
                        if (i < keyFrameGuids.Count)
                        {
                            var kf = x.KeyFrames[i] as KeyFrameData;
                            kf.KeyFrameGuid = keyFrameGuids[i];
                        }
                    }
                }
            }

            if (firstApply)
            {
                nodeMap[x.Id] = node.Id;
                contentGuidList.Add(node.Id);
            }

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

        if (firstApply)
        {
            contentDuplicateGuids = contentGuidList.ToArray();
        }
    }
}
