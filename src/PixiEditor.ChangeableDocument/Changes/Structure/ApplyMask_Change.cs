using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.Drawing;
using Drawie.Backend.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal sealed class ApplyMask_Change : Change
{
    private Guid structureMemberGuid;

    private CommittedChunkStorage? savedChunks;
    private int frame;

    [GenerateMakeChangeAction]
    public ApplyMask_Change(Guid structureMemberGuid, int frame)
    {
        this.structureMemberGuid = structureMemberGuid;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var member = target.FindMember(structureMemberGuid);
        bool isValid = member is not (null or FolderNode) && member.EmbeddedMask is not null;

        return isValid;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var layer = target.FindMemberOrThrow<ImageLayerNode>(structureMemberGuid)!;
        var layerImage = layer.GetLayerImageAtFrame(frame);
        layerImage.EnqueueApplyMask(layer.EmbeddedMask);
        ignoreInUndo = false;
        var layerInfo = new LayerImageArea_ChangeInfo(structureMemberGuid, layerImage.FindAffectedArea());
        savedChunks = new CommittedChunkStorage(layerImage, layerInfo.Area.Chunks);

        layerImage.CommitChanges();
        return layerInfo;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected =
            DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, structureMemberGuid, false, frame,
                ref savedChunks);
        return new LayerImageArea_ChangeInfo(structureMemberGuid, affected);
    }
}
