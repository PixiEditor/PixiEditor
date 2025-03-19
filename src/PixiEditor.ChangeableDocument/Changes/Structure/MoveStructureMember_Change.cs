using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class MoveStructureMember_Change : Change
{
    private Guid memberGuid;

    private Guid targetNodeGuid;

    private Guid originalFolderGuid;

    private ConnectionsData originalConnections;
    private Dictionary<Guid, VecD> originalPositions;

    private bool putInsideFolder;


    [GenerateMakeChangeAction]
    public MoveStructureMember_Change(Guid memberGuid, Guid targetNode, bool putInsideFolder)
    {
        this.memberGuid = memberGuid;
        this.targetNodeGuid = targetNode;
        this.putInsideFolder = putInsideFolder;
    }

    public override bool InitializeAndValidate(Document document)
    {
        var member = document.FindMember(memberGuid);
        var targetFolder = document.FindNode(targetNodeGuid);
        if (member is null || targetFolder is null)
            return false;

        originalConnections = NodeOperations.CreateConnectionsData(member);

        return true;
    }

    private static List<IChangeInfo> Move(Document document, Guid sourceNodeGuid, Guid targetNodeGuid,
        bool putInsideFolder, out Dictionary<Guid, VecD> originalPositions)
    {
        var sourceNode = document.FindMember(sourceNodeGuid);
        var targetNode = document.FindNode(targetNodeGuid);
        originalPositions = null;
        if (sourceNode is null || targetNode is not IRenderInput backgroundInput)
            return [];

        List<IChangeInfo> changes = new();

        Guid oldBackgroundId = sourceNode.Background.Node.Id;

        InputProperty<Painter?> inputProperty = backgroundInput.Background;

        if (targetNode is FolderNode folder && putInsideFolder)
        {
            inputProperty = folder.Content;
        }

        MoveStructureMember_ChangeInfo changeInfo = new(sourceNodeGuid, oldBackgroundId, targetNodeGuid);

        var previouslyConnected = inputProperty.Connection;

        bool isMovingBelow = false;
        
        inputProperty.Node.TraverseForwards(x =>
        {
            if (x.Id == sourceNodeGuid)
            {
                isMovingBelow = true;
                return false;
            }
            
            return true;
        });

        if (isMovingBelow)
        {
            changes.AddRange(NodeOperations.AdjustPositionsBeforeAppend(sourceNode, inputProperty.Node, out originalPositions));
        }

        changes.AddRange(NodeOperations.DetachStructureNode(sourceNode));
        changes.AddRange(NodeOperations.AppendMember(inputProperty, sourceNode.Output,
            sourceNode.Background,
            sourceNode.Id));

        if (!isMovingBelow)
        {
            changes.AddRange(NodeOperations.AdjustPositionsAfterAppend(sourceNode, inputProperty.Node,
                previouslyConnected?.Node as Node, out originalPositions));
        }

        if (targetNode is FolderNode)
        {
            changes.AddRange(AdjustPutIntoFolderPositions(targetNode, originalPositions));
        }

        changes.Add(changeInfo);

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var changes = Move(target, memberGuid, targetNodeGuid, putInsideFolder, out originalPositions);
        ignoreInUndo = false;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        StructureNode member = target.FindMember(memberGuid);

        List<IChangeInfo> changes = new List<IChangeInfo>();

        MoveStructureMember_ChangeInfo changeInfo = new(memberGuid, targetNodeGuid, originalFolderGuid);

        changes.AddRange(NodeOperations.DetachStructureNode(member));
        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(originalConnections, member, target.NodeGraph));
        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));

        changes.Add(changeInfo);

        return changes;
    }
    
    private static List<IChangeInfo> AdjustPutIntoFolderPositions(Node targetNode, Dictionary<Guid, VecD> originalPositions)
    {
        List<IChangeInfo> changes = new();

        if (targetNode is FolderNode folder)
        {
            folder.Content.Connection?.Node.TraverseBackwards(contentNode =>
            {
                if (contentNode is Node node)
                {
                    if (!originalPositions.ContainsKey(node.Id))
                    {
                        originalPositions[node.Id] = node.Position;
                    }
                    
                    node.Position = new VecD(node.Position.X, folder.Position.Y + 250);
                    changes.Add(new NodePosition_ChangeInfo(node.Id, node.Position));
                }
                
                return true;
            });
            
            folder.Background.Connection?.Node.TraverseBackwards(bgNode =>
            {
                if (bgNode is Node node)
                {
                    if (!originalPositions.ContainsKey(node.Id))
                    {
                        originalPositions[node.Id] = node.Position;
                    }

                    double pos = folder.Position.Y;

                    if (folder.Content.Connection != null)
                    {
                        pos -= 250;
                    }
                    
                    node.Position = new VecD(node.Position.X, pos);
                    changes.Add(new NodePosition_ChangeInfo(node.Id, node.Position));
                }
                
                return true;
            });
        }

        return changes;
    }
}
