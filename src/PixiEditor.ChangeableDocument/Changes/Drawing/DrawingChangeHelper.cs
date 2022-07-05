namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal static class DrawingChangeHelper
{
    public static HashSet<VecI> ApplyStoredChunksDisposeAndSetToNull(Document target, Guid memberGuid, bool drawOnMask, ref CommittedChunkStorage? storage)
    {
        var image = GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        return ApplyStoredChunksDisposeAndSetToNull(image, ref storage);
    }

    public static HashSet<VecI> ApplyStoredChunksDisposeAndSetToNull(ChunkyImage image, ref CommittedChunkStorage? storage)
    {
        if (storage is null)
            throw new InvalidOperationException("No stored chunks to apply");
        storage.ApplyChunksToImage(image);
        var chunks = image.FindAffectedChunks();
        image.CommitChanges();
        storage.Dispose();
        storage = null;
        return chunks;
    }

    public static ChunkyImage GetTargetImageOrThrow(Document target, Guid memberGuid, bool drawOnMask)
    {
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
        
        return ((Layer)member).LayerImage;
    }

    public static void ApplyClipsSymmetriesEtc(Document target, ChunkyImage targetImage, Guid targetMemberGuid, bool drawOnMask)
    {
        if (!target.Selection.SelectionPath.IsEmpty)
            targetImage.SetClippingPath(target.Selection.SelectionPath);

        var targetMember = target.FindMemberOrThrow(targetMemberGuid);
        if (targetMember is Layer { LockTransparency: true } && !drawOnMask)
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

    public static OneOf<None, IChangeInfo, List<IChangeInfo>> CreateChunkChangeInfo(Guid memberGuid, HashSet<VecI> affectedChunks, bool drawOnMask) =>
        drawOnMask switch
        {
            false => new LayerImageChunks_ChangeInfo(memberGuid, affectedChunks),
            true => new MaskChunks_ChangeInfo(memberGuid, affectedChunks),
        };
}
