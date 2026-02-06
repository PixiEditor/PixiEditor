using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal static class DrawingChangeHelper
{
    public static AffectedArea ApplyStoredChunksDisposeAndSetToNull(Document target, Guid memberGuid, bool drawOnMask,
        int frame, ref CommittedChunkStorage? storage)
    {
        var image = GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        return ApplyStoredChunksDisposeAndSetToNull(image, ref storage);
    }

    public static AffectedArea ApplyStoredChunksDisposeAndSetToNull(Document target, Guid memberGuid, bool drawOnMask,
        Guid targetKeyFrameGuid, ref CommittedChunkStorage? savedChunks)
    {
        var image = GetTargetImageOrThrow(target, memberGuid, drawOnMask, targetKeyFrameGuid);
        return ApplyStoredChunksDisposeAndSetToNull(image, ref savedChunks);
    }

    public static AffectedArea ApplyStoredChunksDisposeAndSetToNull(ChunkyImage image,
        ref CommittedChunkStorage? storage)
    {
        if (storage is null)
            throw new InvalidOperationException("No stored chunks to apply");
        storage.ApplyChunksToImage(image);
        var area = image.FindAffectedArea();
        image.CommitChanges();
        storage.Dispose();
        storage = null;
        return area;
    }

    public static ChunkyImage GetTargetImageOrThrow(Document target, Guid memberGuid, bool drawOnMask,
        Guid targetKeyFrameGuid)
    {
        var member = target.FindMemberOrThrow(memberGuid);

        if (drawOnMask)
        {
            if (member.EmbeddedMask is null)
                throw new InvalidOperationException("Trying to draw on a mask that doesn't exist");
            return member.EmbeddedMask;
        }

        if (member is FolderNode)
        {
            throw new InvalidOperationException("Trying to draw on a folder");
        }

        if (member is not ImageLayerNode layer)
        {
            throw new InvalidOperationException("Trying to draw on a non-raster layer member");
        }

        return layer.GetLayerImageByKeyFrameGuid(targetKeyFrameGuid);
    }

    public static ChunkyImage? GetTargetImageOrThrow(Document target, Guid memberGuid, bool drawOnMask, int frame)
    {
        // TODO: Figure out if this should work only for raster layers or should rasterize any
        var member = target.FindMemberOrThrow(memberGuid);

        if (drawOnMask)
        {
            if (member.EmbeddedMask is null)
                throw new InvalidOperationException("Trying to draw on a mask that doesn't exist");
            return member.EmbeddedMask;
        }

        if (member is FolderNode)
        {
            throw new InvalidOperationException("Trying to draw on a folder");
        }

        if (member is not ImageLayerNode layer)
        {
            throw new InvalidOperationException("Trying to draw on a non-raster layer member");
        }

        return layer.GetLayerImageAtFrame(frame);
    }

    public static void ApplyClipsSymmetriesEtc(Document target, ChunkyImage targetImage, Guid targetMemberGuid,
        bool drawOnMask)
    {
        if (!target.Selection.SelectionPath.IsEmpty)
            targetImage.SetClippingPath(target.Selection.SelectionPath);

        var targetMember = target.FindMemberOrThrow(targetMemberGuid);
        if (targetMember is ITransparencyLockable { LockTransparency: true } && !drawOnMask)
            targetImage.EnableLockTransparency();

        if (target.HorizontalSymmetryAxisEnabled)
            targetImage.SetHorizontalAxisOfSymmetry(target.HorizontalSymmetryAxisY);
        if (target.VerticalSymmetryAxisEnabled)
            targetImage.SetVerticalAxisOfSymmetry(target.VerticalSymmetryAxisX);
    }

    public static bool IsValidForDrawing(Document target, Guid memberGuid, bool drawOnMask, int frame)
    {
        if (!target.TryFindMember(memberGuid, out var member))
        {
            return false;
        }

        if (!drawOnMask && member is ImageLayerNode layerNode)
        {
            return layerNode.GetLayerImageAtFrame(frame) is not null;
        }

        return drawOnMask switch
        {
            // If it should draw on the mask, the mask can't be null
            true when member.EmbeddedMask is null => false,
            // If it should not draw on the mask, the member can't be a folder
            false when member is not ImageLayerNode => false,
            _ => true
        };
    }

    public static OneOf<None, IChangeInfo, List<IChangeInfo>> CreateAreaChangeInfo(Guid memberGuid,
        AffectedArea affectedArea, bool drawOnMask) =>
        drawOnMask switch
        {
            false => new LayerImageArea_ChangeInfo(memberGuid, affectedArea),
            true => new MaskArea_ChangeInfo(memberGuid, affectedArea),
        };

    public static bool IsValidForDrawing(Document target, Guid memberGuid, bool drawOnMask, Guid targetKeyFrameGuid)
    {
        if (!target.TryFindMember(memberGuid, out var member))
        {
            return false;
        }

        if (!drawOnMask && member is ImageLayerNode layerNode)
        {
            return layerNode.GetLayerImageByKeyFrameGuid(targetKeyFrameGuid) is not null;
        }

        return drawOnMask switch
        {
            // If it should draw on the mask, the mask can't be null
            true when member.EmbeddedMask is null => false,
            // If it should not draw on the mask, the member can't be a folder
            false when member is not ImageLayerNode => false,
            _ => true
        };
    }
}
