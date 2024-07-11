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

    private List<IInputProperty> originalOutputConnections = new();
    private List<(IInputProperty, IOutputProperty?)> originalInputConnections = new();
    
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

        originalOutputConnections = member.Output.Connections.ToList();
        originalInputConnections = member.InputProperties.Select(x => ((IInputProperty)x, x.Connection)).ToList();
        return true;
    }

    private static List<IChangeInfo> Move(Document document, Guid sourceNodeGuid, Guid targetNodeGuid, bool putInsideFolder)
    {
        var sourceNode = document.FindMember(sourceNodeGuid);
        var targetNode = document.FindNode(targetNodeGuid);
        if (sourceNode is null || targetNode is not IBackgroundInput backgroundInput)
            return [];

        List<IChangeInfo> changes = new();

        InputProperty<ChunkyImage?> inputProperty = backgroundInput.Background;

        if (targetNode is FolderNode folder && putInsideFolder)
        {
            inputProperty = folder.Content;
        }

        changes.AddRange(NodeOperations.DetachStructureNode(sourceNode));
        changes.AddRange(NodeOperations.AppendMember(inputProperty, sourceNode.Output,
            sourceNode.Background,
            sourceNode.Id));

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
        
        changes.AddRange(NodeOperations.DetachStructureNode(member));
        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(originalOutputConnections,
            originalInputConnections, member));
        return changes;
    }
}
