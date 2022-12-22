using PixiEditor.ChangeableDocument.Changes.Drawing;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal sealed class ApplyMask_Change : Change
{
    private Guid structureMemberGuid;
    private Layer? layer;
    
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
        if (isValid)
        {
            layer = member as Layer;
        }
        
        return isValid;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        layer!.LayerImage.EnqueueApplyMask(layer.Mask!);
        ignoreInUndo = false;
        var layerInfo = new LayerImageChunks_ChangeInfo(structureMemberGuid, layer.LayerImage.FindAffectedChunks());
        savedChunks = new CommittedChunkStorage(layer.LayerImage, layerInfo.Chunks);
        
        layer.LayerImage.CommitChanges();
        return layerInfo;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, structureMemberGuid, false, ref savedChunks);
        return new LayerImageChunks_ChangeInfo(structureMemberGuid, affected);
    }
}
