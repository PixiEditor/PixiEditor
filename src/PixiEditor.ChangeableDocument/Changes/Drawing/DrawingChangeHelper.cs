namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal static class DrawingChangeHelper
{
    public static ChunkyImage GetTargetImageOrThrow(Document target, Guid memberGuid, bool drawOnMask)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        if (drawOnMask)
        {
            if (member.Mask is null)
                throw new InvalidOperationException("Trying to draw on a mask that doesn't exist");
            return member.Mask;
        }
        else if (member is Folder)
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
        if (targetMember is Layer layer && layer.LockTransparency && !drawOnMask)
            targetImage.EnableLockTransparency();

        if (target.HorizontalSymmetryAxisEnabled)
            targetImage.SetHorizontalAxisOfSymmetry(target.HorizontalSymmetryAxisY);
        if (target.VerticalSymmetryAxisEnabled)
            targetImage.SetVerticalAxisOfSymmetry(target.VerticalSymmetryAxisX);
    }

    public static bool IsValidForDrawing(Document target, Guid memberGuid, bool drawOnMask)
    {
        var member = target.FindMember(memberGuid);
        if (member is null)
            return false;
        if (drawOnMask && member.Mask is null)
            return false;
        if (!drawOnMask && member is Folder)
            return false;
        return true;
    }

    public static OneOf<None, IChangeInfo, List<IChangeInfo>> CreateChunkChangeInfo(Guid memberGuid, HashSet<VecI> affectedChunks, bool drawOnMask)
    {
        return drawOnMask switch
        {
            false => new LayerImageChunks_ChangeInfo(memberGuid, affectedChunks),
            true => new MaskChunks_ChangeInfo(memberGuid, affectedChunks),
        };
    }
}
