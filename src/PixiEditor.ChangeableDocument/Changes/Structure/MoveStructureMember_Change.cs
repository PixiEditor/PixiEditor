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
    
    private Guid originalParentGuid;

    [GenerateMakeChangeAction]
    public MoveStructureMember_Change(Guid memberGuid, Guid targetNode)
    {
        this.memberGuid = memberGuid;
        this.targetNodeGuid = targetNode;
    }

    public override bool InitializeAndValidate(Document document)
    {
        var member = document.FindMember(memberGuid);
        var targetFolder = document.FindNode(targetNodeGuid);
        if (member is null || targetFolder is null)
            return false;

        member.TraverseForwards(node =>
        {
            originalParentGuid = node.Id;
            return false;
        });
        
        return true;
    }

    private static List<IChangeInfo> Move(Document document, Guid sourceNodeGuid, Guid targetNodeGuid)
    {
        var sourceNode = document.FindMember(sourceNodeGuid);
        var targetNode = document.FindNode(targetNodeGuid);
        if (sourceNode is null || targetNode is not IBackgroundInput backgroundInput)
            return [];
        
        List<IChangeInfo> changes = new();
        
        changes.AddRange(NodeOperations.DetachStructureNode(sourceNode));
        changes.AddRange(NodeOperations.AppendMember(backgroundInput.Background, sourceNode.Output, sourceNode.Background,
            sourceNode.Id));
        
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var changes = Move(target, memberGuid, targetNodeGuid);
        ignoreInUndo = false;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        // TODO: this is lossy, original connections might be lost
        var changes = Move(target, memberGuid, originalFolderGuid);
        return changes; 
    }
}
