using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class MoveStructureMember_Change : Change
{
    private Guid memberGuid;

    private Guid targetNodeGuid;

    private Guid originalFolderGuid;

    private List<PropertyConnection> originalOutputConnections = new();
    private List<(PropertyConnection input, PropertyConnection? output)> originalInputConnections = new();
    
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

        originalOutputConnections = member.Output.Connections.Select(x => new PropertyConnection(x.Node.Id, x.InternalPropertyName))
            .ToList();
        
        originalInputConnections = member.InputProperties.Select(x => 
            (new PropertyConnection(x.Node.Id, x.InternalPropertyName), new PropertyConnection(x.Connection?.Node.Id, x.Connection?.InternalPropertyName)))
            .ToList();
          
        return true;
    }

    private static List<IChangeInfo> Move(Document document, Guid sourceNodeGuid, Guid targetNodeGuid, bool putInsideFolder)
    {
        var sourceNode = document.FindMember(sourceNodeGuid);
        var targetNode = document.FindNode(targetNodeGuid);
        if (sourceNode is null || targetNode is not IBackgroundInput backgroundInput)
            return [];

        List<IChangeInfo> changes = new();
        
        Guid oldBackgroundId = sourceNode.Background.Node.Id;

        InputProperty<Surface?> inputProperty = backgroundInput.Background;

        if (targetNode is FolderNode folder && putInsideFolder)
        {
            inputProperty = folder.Content;
        }

        MoveStructureMember_ChangeInfo changeInfo = new(sourceNodeGuid, oldBackgroundId, targetNodeGuid);
        
        changes.AddRange(NodeOperations.DetachStructureNode(sourceNode));
        changes.AddRange(NodeOperations.AppendMember(inputProperty, sourceNode.Output,
            sourceNode.Background,
            sourceNode.Id));
        
        changes.Add(changeInfo);

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var changes = Move(target, memberGuid, targetNodeGuid, putInsideFolder);
        ignoreInUndo = false;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        StructureNode member = target.FindMember(memberGuid);

        List<IChangeInfo> changes = new List<IChangeInfo>();
        
        MoveStructureMember_ChangeInfo changeInfo = new(memberGuid, targetNodeGuid, originalFolderGuid);
        
        changes.AddRange(NodeOperations.DetachStructureNode(member));
        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(
            originalOutputConnections, originalInputConnections, member, target.NodeGraph));
        
        changes.Add(changeInfo);
        
        return changes;
    }
}
