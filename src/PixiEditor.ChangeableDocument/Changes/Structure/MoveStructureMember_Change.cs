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
        var targetNode = document.FindNode(targetNodeGuid);
        if (member is null || targetNode is null)
            return false;

        if (WillCreateLoop(member, targetNode))
        {
            FailedMessage = "ERROR_LOOP_DETECTED_MESSAGE";
            return false;
        }

        originalConnections = NodeOperations.CreateConnectionsData(member);

        return true;
    }

    private static List<IChangeInfo> Move(Document document, Guid nodeBeingMovedGuid, Guid targetNodeGuid,
        bool putInsideFolder, out Dictionary<Guid, VecD> originalPositions)
    {
        var nodeBeingMoved = document.FindMember(nodeBeingMovedGuid);
        var targetNode = document.FindNode(targetNodeGuid);
        originalPositions = null;

        if (nodeBeingMoved is null)
            return [];

        List<IChangeInfo> changes = new();

        Guid oldBackgroundId = nodeBeingMoved.Background.Node.Id;

        var potentialInputProperties = targetNode.InputProperties.Where(x => x.ValueType == typeof(Painter)).ToArray();
        InputProperty<Painter?> inputProperty = targetNode is IRenderInput renderInput ? renderInput.Background : null;
        if (inputProperty == null)
        {
            foreach (var potentialInputProperty in potentialInputProperties)
            {
                bool traversesBackToSource = false;

                potentialInputProperty.Connection?.Node.TraverseBackwards((x, prop) =>
                {
                    if (x.Id == nodeBeingMovedGuid)
                    {
                        traversesBackToSource = true;
                        return false;
                    }

                    return true;
                });

                if (!traversesBackToSource)
                {
                    potentialInputProperty.Connection?.Node.TraverseForwards((x, prop) =>
                    {
                        if (x.Id == nodeBeingMovedGuid)
                        {
                            traversesBackToSource = true;
                            return false;
                        }

                        return true;
                    });
                }

                if (traversesBackToSource)
                {
                    inputProperty = potentialInputProperty as InputProperty<Painter?>;
                    break;
                }
            }

            var firstPotential = potentialInputProperties.FirstOrDefault();
            if (inputProperty == null && firstPotential?.Connection == null)
            {
                inputProperty = firstPotential as InputProperty<Painter?>;
            }
        }

        if(inputProperty is null || (inputProperty.Connection?.Node == nodeBeingMoved && !putInsideFolder)) return [];

        if (targetNode is FolderNode folder && putInsideFolder)
        {
            inputProperty = folder.Content;
        }

        Node? oldInputConnectionNode = nodeBeingMoved.Background.Connection?.Node as Node;
        Node oldOutputConnectionNode = nodeBeingMoved.Output.Connections.First().Node as Node;
        bool hadMultipleOutputs = nodeBeingMoved.Output.Connections.Count > 1;
        
        MoveStructureMember_ChangeInfo changeInfo = new(nodeBeingMovedGuid, oldOutputConnectionNode.Id, targetNodeGuid);

        changes.AddRange(NodeOperations.DetachStructureNode(nodeBeingMoved));
        changes.AddRange(NodeOperations.AppendMember(inputProperty, nodeBeingMoved.Output,
            nodeBeingMoved.Background,
            nodeBeingMoved.Id));

        originalPositions = new();
        if (!hadMultipleOutputs && oldInputConnectionNode is not null)
        {
            changes.AddRange(NodeOperations.CollapseFreeSpaceAfterRemovingNode(oldInputConnectionNode, oldOutputConnectionNode, out var tempOriginalPositions));
            foreach (var (key, value) in tempOriginalPositions)
            {
                originalPositions.TryAdd(key, value);
            }
        }

        double verticalOffset = 0;
        if (targetNode is FolderNode && putInsideFolder)
            verticalOffset = 280;
        else if (targetNode is FolderNode)
            verticalOffset = -280;
        
        VecD sourceOriginalPosition = nodeBeingMoved.Position;
        changes.AddRange(NodeOperations.PushNodesBackAfterInsertingNodeBetweenTwoOthers(nodeBeingMoved, inputProperty.Node,
            nodeBeingMoved.Background.Connection?.Node as Node, out var tempOriginalPositions2, verticalOffset));
        foreach (var (key, value) in tempOriginalPositions2)
        {
            originalPositions.TryAdd(key, value);
        }
        originalPositions[nodeBeingMoved.Id] = sourceOriginalPosition;
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

    private bool WillCreateLoop(StructureNode member, Node targetNode)
    {
        InputProperty? input = targetNode.GetInputProperty("Background");
        OutputProperty output = member.Output;

        if (input is null)
            return false;

        return IsLoop(input, output);
    }

    private static bool IsLoop(InputProperty input, OutputProperty output)
    {
        if (input.Node == output.Node)
        {
            return true;
        }

        if (input.Node.OutputProperties.Any(x => x.InternalPropertyName != "Output" && x.Connections.Any(y => y.Node == output.Node)))
        {
            return true;
        }

        bool isLoop = false;
        input.Node.TraverseForwards((node, inputProp) =>
        {
            if (node == output.Node && inputProp.InternalPropertyName != "Background")
            {
                isLoop = true;
                return false;
            }

            return true;
        });

        return isLoop;
    }
}
