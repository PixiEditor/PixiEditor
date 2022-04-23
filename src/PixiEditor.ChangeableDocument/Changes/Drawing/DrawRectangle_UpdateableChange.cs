using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class DrawRectangle_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private ShapeData rect;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? storedChunks;
    public DrawRectangle_UpdateableChange(Guid memberGuid, ShapeData rectangle, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.rect = rectangle;
        this.drawOnMask = drawOnMask;
    }

    public override void Initialize(Document target) { }

    public void Update(ShapeData updatedRectangle)
    {
        rect = updatedRectangle;
    }

    private ChunkyImage GetTargetImage(Document target)
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

    public override IChangeInfo? ApplyTemporarily(Document target)
    {
        ChunkyImage targetImage = GetTargetImage(target);

        var oldChunks = targetImage.FindAffectedChunks();
        targetImage.CancelChanges();
        if (!target.Selection.IsEmptyAndInactive)
            targetImage.ApplyRasterClip(target.Selection.SelectionImage);
        targetImage.DrawRectangle(rect);
        var newChunks = targetImage.FindAffectedChunks();
        newChunks.UnionWith(oldChunks);

        return drawOnMask switch
        {
            false => new LayerImageChunks_ChangeInfo()
            {
                Chunks = newChunks,
                LayerGuid = memberGuid
            },
            true => new MaskChunks_ChangeInfo()
            {
                Chunks = newChunks,
                MemberGuid = memberGuid
            },
        };
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        ChunkyImage targetImage = GetTargetImage(target);
        var changes = ApplyTemporarily(target);

        var changedChunks = changes! switch
        {
            LayerImageChunks_ChangeInfo info => info.Chunks,
            MaskChunks_ChangeInfo info => info.Chunks,
            _ => throw new InvalidOperationException("Unknown chunk type"),
        };

        storedChunks = new CommittedChunkStorage(targetImage, changedChunks!);
        targetImage.CommitChanges();

        ignoreInUndo = false;
        return changes;
    }

    public override IChangeInfo? Revert(Document target)
    {
        ChunkyImage targetImage = GetTargetImage(target);
        storedChunks!.ApplyChunksToImage(targetImage);
        storedChunks.Dispose();
        storedChunks = null;
        IChangeInfo changes = drawOnMask switch
        {
            false => new LayerImageChunks_ChangeInfo()
            {
                Chunks = targetImage.FindAffectedChunks(),
                LayerGuid = memberGuid,
            },
            true => new MaskChunks_ChangeInfo()
            {
                Chunks = targetImage.FindAffectedChunks(),
                MemberGuid = memberGuid,
            },
        };
        targetImage.CommitChanges();
        return changes;
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
