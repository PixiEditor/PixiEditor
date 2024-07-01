using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal static class DrawingChangeHelper
{
    public static AffectedArea ApplyStoredChunksDisposeAndSetToNull(Document target, Guid memberGuid, bool drawOnMask, ref CommittedChunkStorage? storage)
    {
        var image = GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        return ApplyStoredChunksDisposeAndSetToNull(image, ref storage);
    }

    public static AffectedArea ApplyStoredChunksDisposeAndSetToNull(ChunkyImage image, ref CommittedChunkStorage? storage)
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

    public static ChunkyImage GetTargetImageOrThrow(Document target, Guid memberGuid, bool drawOnMask, int frame)
    {
        // TODO: Figure out if this should work only for raster layers or should rasterize any
        var member = target.FindMemberOrThrow(memberGuid);
        
        if (drawOnMask)
        {
            if (member.Mask is null)
                throw new InvalidOperationException("Trying to draw on a mask that doesn't exist");
            return member.Mask;
        }
        
        if (member is Folder)
        {
            throw new InvalidOperationException("Trying to draw on a folder");
        }

        if (member is not RasterLayer layer)
        {
            throw new InvalidOperationException("Trying to draw on a non-raster layer member");
        }
        
        return layer.GetLayerImageAtFrame(frame);
    }

    public static void ApplyClipsSymmetriesEtc(Document target, ChunkyImage targetImage, Guid targetMemberGuid, bool drawOnMask)
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

    public static bool IsValidForDrawing(Document target, Guid memberGuid, bool drawOnMask)
    {
        if (!target.TryFindMember(memberGuid, out var member))
        {
            return false;
        }

        return drawOnMask switch
        {
            // If it should draw on the mask, the mask can't be null
            true when member.Mask is null => false,
            // If it should not draw on the mask, the member can't be a folder
            false when member is Folder => false,
            _ => true
        };
    }

    public static OneOf<None, IChangeInfo, List<IChangeInfo>> CreateAreaChangeInfo(Guid memberGuid, AffectedArea affectedArea, bool drawOnMask) =>
        drawOnMask switch
        {
            false => new LayerImageArea_ChangeInfo(memberGuid, affectedArea),
            true => new MaskArea_ChangeInfo(memberGuid, affectedArea),
        };
}
