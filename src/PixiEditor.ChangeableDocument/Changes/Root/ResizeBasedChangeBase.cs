using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal abstract class ResizeBasedChangeBase : Change
{
    protected VecI _originalSize;
    protected int _originalHorAxisY;
    protected int _originalVerAxisX;
    protected Dictionary<Guid, CommittedChunkStorage> deletedChunks = new();
    protected Dictionary<Guid, CommittedChunkStorage> deletedMaskChunks = new();
    
    public override bool InitializeAndValidate(Document target)
    {
        _originalSize = target.Size;
        _originalHorAxisY = target.HorizontalSymmetryAxisY;
        _originalVerAxisX = target.VerticalSymmetryAxisX;
        return true;
    }
    
    protected void Resize(ChunkyImage img, Guid memberGuid, VecI size, VecI offset, Dictionary<Guid, CommittedChunkStorage> deletedChunksDict)
    {
        img.EnqueueResize(size);
        img.EnqueueClear();
        img.EnqueueDrawChunkyImage(offset, img);

        deletedChunksDict.Add(memberGuid, new CommittedChunkStorage(img, img.FindAffectedChunks()));
        img.CommitChanges();
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (target.Size == _originalSize)
            return new None();

        target.Size = _originalSize;
        target.ForEveryMember((member) =>
        {
            if (member is Layer layer)
            {
                layer.LayerImage.EnqueueResize(_originalSize);
                deletedChunks[layer.GuidValue].ApplyChunksToImage(layer.LayerImage);
                layer.LayerImage.CommitChanges();
            }
            
            if (member.Mask is null)
                return;
            member.Mask.EnqueueResize(_originalSize);
            deletedMaskChunks[member.GuidValue].ApplyChunksToImage(member.Mask);
            member.Mask.CommitChanges();
        });

        target.HorizontalSymmetryAxisY = _originalHorAxisY;
        target.VerticalSymmetryAxisX = _originalVerAxisX;

        DisposeDeletedChunks();

        return new Size_ChangeInfo(_originalSize, _originalVerAxisX, _originalHorAxisY);
    }
    
    private void DisposeDeletedChunks()
    {
        foreach (var stored in deletedChunks)
            stored.Value.Dispose();
        deletedChunks = new();

        foreach (var stored in deletedMaskChunks)
            stored.Value.Dispose();
        deletedMaskChunks = new();
    }
    
    public override void Dispose()
    {
        DisposeDeletedChunks();
    }
}
