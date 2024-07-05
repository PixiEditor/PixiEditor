using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class DeleteStructureMember_Change : Change
{
    private Guid memberGuid;
    private Guid parentGuid;
    private int originalIndex;
    private StructureNode? savedCopy;

    [GenerateMakeChangeAction]
    public DeleteStructureMember_Change(Guid memberGuid)
    {
        this.memberGuid = memberGuid;
    }

    public override bool InitializeAndValidate(Document document)
    {
        var (member, parent) = document.FindChildAndParent(memberGuid);
        if (member is null || parent is null)
            return false;

        //originalIndex = parent.Children.IndexOf(member);
        parentGuid = parent.Id;
        savedCopy = (StructureNode)member.Clone();
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply, out bool ignoreInUndo)
    {
        /*var (member, parent) = document.FindChildAndParentOrThrow(memberGuid);
        parent.Children = parent.Children.Remove(member);
        member.Dispose();
        ignoreInUndo = false;
        return new DeleteStructureMember_ChangeInfo(memberGuid, parentGuid);*/
        
        ignoreInUndo = false;
        return new None();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document doc)
    {
        /*var parent = doc.FindMemberOrThrow<FolderNode>(parentGuid);

        var copy = savedCopy!.Clone();
        parent.Children = parent.Children.Insert(originalIndex, copy);
        return copy switch
        {
            LayerNode => CreateLayer_ChangeInfo.FromLayer(parentGuid, originalIndex, (LayerNode)copy),
            FolderNode => CreateFolder_ChangeInfo.FromFolder(parentGuid, originalIndex, (FolderNode)copy),
            _ => throw new NotSupportedException(),
        };*/
        
        return new None();
    }

    public override void Dispose()
    {
        savedCopy?.Dispose();
    }
}
