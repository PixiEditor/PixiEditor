using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
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
    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember<LayerNode>(layerGuid, out LayerNode? layer))
            return false;
        duplicateGuid = Guid.NewGuid();
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        (LayerNode existingLayer, FolderNode parent) = ((LayerNode, FolderNode))target.FindChildAndParentOrThrow(layerGuid);

        LayerNode clone = (LayerNode)existingLayer.Clone();
        clone.Id = duplicateGuid;

        /*int index = parent.Children.IndexOf(existingLayerNode);
        parent.Children = parent.Children.Insert(index, clone);

        ignoreInUndo = false;
        return CreateLayer_ChangeInfo.FromLayer(parent.Id, index, clone);*/
//TODO: Implement
        ignoreInUndo = false;
        return new None();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        /*var (member, parent) = target.FindChildAndParentOrThrow(duplicateGuid);
        parent.Children = parent.Children.Remove(member);
        member.Dispose();
        return new DeleteStructureMember_ChangeInfo(duplicateGuid, parent.Id);*/

        return new None();
    }
}
