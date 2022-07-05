using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ResizeCanvas_Change : Change
{
    private VecI originalSize;
    private int originalHorAxisY;
    private int originalVerAxisX;
    private Dictionary<Guid, CommittedChunkStorage> deletedChunks = new();
    private Dictionary<Guid, CommittedChunkStorage> deletedMaskChunks = new();
    private VecI newSize;
    private readonly ResizeAnchor anchor;

    [GenerateMakeChangeAction]
    public ResizeCanvas_Change(VecI size, ResizeAnchor anchor)
    {
        newSize = size;
        this.anchor = anchor;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.Size == newSize || newSize.X < 1 || newSize.Y < 1)
            return new Error();
        originalSize = target.Size;
        originalHorAxisY = target.HorizontalSymmetryAxisY;
        originalVerAxisX = target.VerticalSymmetryAxisX;
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.Size = newSize;
        target.VerticalSymmetryAxisX = Math.Clamp(originalVerAxisX, 0, target.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(originalHorAxisY, 0, target.Size.Y);

        target.ForEveryMember((member) =>
        {
            if (member is Layer layer)
            {
                layer.LayerImage.EnqueueResize(newSize);
                layer.LayerImage.EnqueueClear();
                layer.LayerImage.EnqueueDrawChunkyImage(anchor.FindOffsetFor(originalSize, newSize), layer.LayerImage);

                deletedChunks.Add(layer.GuidValue, new CommittedChunkStorage(layer.LayerImage, layer.LayerImage.FindAffectedChunks()));
                layer.LayerImage.CommitChanges();
            }
            if (member.Mask is null)
                return;

            member.Mask.EnqueueResize(newSize);
            member.Mask.EnqueueClear();
            member.Mask.EnqueueDrawChunkyImage(anchor.FindOffsetFor(originalSize, newSize), member.Mask);
            deletedMaskChunks.Add(member.GuidValue, new CommittedChunkStorage(member.Mask, member.Mask.FindAffectedChunks()));
            member.Mask.CommitChanges();
        });
        ignoreInUndo = false;
        return new Size_ChangeInfo(newSize, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Size = originalSize;
        target.ForEveryMember((member) =>
        {
            if (member is Layer layer)
            {
                layer.LayerImage.EnqueueResize(originalSize);
                deletedChunks[layer.GuidValue].ApplyChunksToImage(layer.LayerImage);
                layer.LayerImage.CommitChanges();
            }
            
            if (member.Mask is null)
                return;
            member.Mask.EnqueueResize(originalSize);
            deletedMaskChunks[member.GuidValue].ApplyChunksToImage(member.Mask);
            member.Mask.CommitChanges();
        });

        target.HorizontalSymmetryAxisY = originalHorAxisY;
        target.VerticalSymmetryAxisX = originalVerAxisX;

        foreach (var stored in deletedChunks)
            stored.Value.Dispose();
        deletedChunks = new();
        
        foreach (var stored in deletedMaskChunks)
            stored.Value.Dispose();
        deletedMaskChunks = new();

        return new Size_ChangeInfo(originalSize, originalVerAxisX, originalHorAxisY);
    }

    public override void Dispose()
    {
        foreach (var layer in deletedChunks)
            layer.Value.Dispose();
        foreach (var mask in deletedMaskChunks)
            mask.Value.Dispose();
    }
}
