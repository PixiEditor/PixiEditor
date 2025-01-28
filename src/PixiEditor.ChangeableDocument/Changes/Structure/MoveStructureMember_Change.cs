using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
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

    private static List<IChangeInfo> Move(Document document, Guid sourceNodeGuid, Guid targetNodeGuid, bool putInsideFolder, out Dictionary<Guid, VecD> originalPositions)
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
        
        changes.AddRange(NodeOperations.DetachStructureNode(sourceNode));
        changes.AddRange(NodeOperations.AppendMember(inputProperty, sourceNode.Output,
            sourceNode.Background,
            sourceNode.Id));
        
        changes.AddRange(NodeOperations.AdjustPositionsAfterAppend(sourceNode, inputProperty.Node, previouslyConnected?.Node as Node, out originalPositions));
        
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
}
