using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Common;

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

    /// <summary>
    /// Looks through all the painter inputs of <paramref name="targetDescendantNode"/>
    /// and selects the best one for the moved node to be attached to
    /// </summary>
    /// <param name="nodeBeingMovedGuid"></param>
    /// <param name="targetDescendantNode">The node that will come right after nodeBeingMoved after the move occurs</param>
    /// <returns></returns>
    private static InputProperty<Painter?>? FindInsertionLocation(Guid nodeBeingMovedGuid, Node targetDescendantNode)
    {
        var potentialInputProperties = targetDescendantNode.InputProperties.Where(x => x.ValueType == typeof(Painter)).ToArray();
        InputProperty<Painter?> inputProperty = targetDescendantNode is IRenderInput renderInput ? renderInput.Background : null;
        if (inputProperty != null)
            return inputProperty;
        
        foreach (var potentialInputProperty in potentialInputProperties)
        {
            var targetAncestorNode = potentialInputProperty.Connection?.Node;
            bool traversesToNodeBeingMoved = false;
            
            targetAncestorNode?.TraverseBackwards((x, prop) =>
            {
                if (x.Id == nodeBeingMovedGuid)
                {
                    traversesToNodeBeingMoved = true;
                    return false;
                }

                return true;
            });

            if (!traversesToNodeBeingMoved)
            {
                targetAncestorNode?.TraverseForwards((x, prop) =>
                {
                    if (x.Id == nodeBeingMovedGuid)
                    {
                        traversesToNodeBeingMoved = true;
                        return false;
                    }

                    return true;
                });
            }

            if (traversesToNodeBeingMoved)
            {
                inputProperty = potentialInputProperty as InputProperty<Painter?>;
                break;
            }
        }

        if (inputProperty != null)
            return inputProperty;

        var firstPotential = potentialInputProperties.FirstOrDefault();
        if (firstPotential?.Connection == null)
            return firstPotential as InputProperty<Painter?>;
        
        return null;
    }

    private static List<IChangeInfo> Move(Document document, Guid nodeBeingMovedGuid, Guid targetDescendantNodeGuid,
        bool putInsideFolder, out Dictionary<Guid, VecD> originalPositions)
    {
        var nodeBeingMoved = document.FindMember(nodeBeingMovedGuid);
        var targetDescendantNode = document.FindNode(targetDescendantNodeGuid);
        originalPositions = null;

        if (nodeBeingMoved is null)
            return [];

        InputProperty<Painter?> inputProperty;
        if (targetDescendantNode is FolderNode folder && putInsideFolder)
        {
            inputProperty = folder.Content;
        }
        else
        {
            inputProperty = FindInsertionLocation(nodeBeingMovedGuid, targetDescendantNode);

            if (inputProperty is null || (inputProperty.Connection?.Node == nodeBeingMoved && !putInsideFolder))
                return [];
        }


        Node? oldInputConnectionNode = nodeBeingMoved.Background.Connection?.Node as Node;
        Node oldOutputConnectionNode = nodeBeingMoved.Output.Connections.First().Node as Node;
        bool hadMultipleOutputs = nodeBeingMoved.Output.Connections.Count > 1;
        
        MoveStructureMember_ChangeInfo changeInfo = 
            new(nodeBeingMovedGuid, oldOutputConnectionNode.Id, targetDescendantNodeGuid);

        List<IChangeInfo> changes = new();
        changes.AddRange(NodeOperations.DetachStructureNode(nodeBeingMoved));
        changes.AddRange(NodeOperations.AppendMember(inputProperty, nodeBeingMoved.Output,
            nodeBeingMoved.Background,
            nodeBeingMoved.Id));

        originalPositions = new();
        if (!hadMultipleOutputs && oldInputConnectionNode is not null)
        {
            changes.AddRange(NodeOperations.CollapseFreeSpaceAfterRemovingNode(oldInputConnectionNode, oldOutputConnectionNode, out var tempOriginalPositions));
            originalPositions.AddRangeNewOnly(tempOriginalPositions);
        }

        double verticalOffset = 0;
        if (targetDescendantNode is FolderNode && putInsideFolder)
            verticalOffset = 550;
        
        VecD sourceOriginalPosition = nodeBeingMoved.Position;
        changes.AddRange(NodeOperations.PushNodesBackAfterInsertingNodeBetweenTwoOthers(nodeBeingMoved, inputProperty.Node,
            nodeBeingMoved.Background.Connection?.Node as Node, out var tempOriginalPositions2, verticalOffset));
        originalPositions.AddRangeNewOnly(tempOriginalPositions2);
        originalPositions[nodeBeingMoved.Id] = sourceOriginalPosition;

        if (nodeBeingMoved is FolderNode folderNode)
        {
            changes.AddRange(NodeOperations.MoveFolderContentAfterFolderMovement(folderNode, sourceOriginalPosition,
                out var tempOriginalPositions3));
            originalPositions.AddRangeNewOnly(tempOriginalPositions3);
            originalPositions[nodeBeingMoved.Id] = sourceOriginalPosition;
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
