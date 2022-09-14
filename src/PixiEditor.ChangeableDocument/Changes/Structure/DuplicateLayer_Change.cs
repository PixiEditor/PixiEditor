using PixiEditor.ChangeableDocument.ChangeInfos.Structure;

namespace PixiEditor.ChangeableDocument.Changes.Structure;
internal class DuplicateLayer_Change : Change
{
    private readonly Guid layerGuid;
    private Guid duplicateGuid;

    [GenerateMakeChangeAction]
    public DuplicateLayer_Change(Guid layerGuid)
    {
        this.layerGuid = layerGuid;
    }
    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember<Layer>(layerGuid, out Layer? layer))
            return new Error();
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        (Layer existingLayer, Folder parent) = ((Layer, Folder))target.FindChildAndParentOrThrow(layerGuid);

        Layer clone = existingLayer.Clone();
        duplicateGuid = Guid.NewGuid();
        clone.GuidValue = duplicateGuid;

        int index = parent.Children.IndexOf(existingLayer);
        parent.Children = parent.Children.Insert(index, clone);

        ignoreInUndo = false;
        return CreateLayer_ChangeInfo.FromLayer(parent.GuidValue, index, clone);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var (member, parent) = target.FindChildAndParentOrThrow(duplicateGuid);
        parent.Children = parent.Children.Remove(member);
        member.Dispose();
        return new DeleteStructureMember_ChangeInfo(duplicateGuid, parent.GuidValue);
    }
}
