using PixiEditor.ChangeableDocument.Changes.Drawing;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal sealed class ApplyMask_Change : Change
{
    private Guid structureMemberGuid;

    private CommittedChunkStorage? savedChunks;

    [GenerateMakeChangeAction]
    public ApplyMask_Change(Guid structureMemberGuid)
    {
        this.structureMemberGuid = structureMemberGuid;
    }
        
    public override bool InitializeAndValidate(Document target)
    {
        var member = target.FindMember(structureMemberGuid);
        bool isValid = member is not (null or Folder) && member.Mask is not null;

        return isValid;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var layer = (Layer)target.FindMember(structureMemberGuid)!;
        layer!.LayerImage.EnqueueApplyMask(layer.Mask!);
        ignoreInUndo = false;
        var layerInfo = new LayerImageArea_ChangeInfo(structureMemberGuid, layer.LayerImage.FindAffectedArea());
        savedChunks = new CommittedChunkStorage(layer.LayerImage, layerInfo.Area.Chunks);
        
        layer.LayerImage.CommitChanges();
        return layerInfo;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, structureMemberGuid, false, ref savedChunks);
        return new LayerImageArea_ChangeInfo(structureMemberGuid, affected);
    }
}
