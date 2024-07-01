using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal abstract class ResizeBasedChangeBase : Change
{
    protected VecI _originalSize;
    protected double _originalHorAxisY;
    protected double _originalVerAxisX;
    protected Dictionary<Guid, CommittedChunkStorage> deletedChunks = new();
    protected Dictionary<Guid, CommittedChunkStorage> deletedMaskChunks = new();
    protected int frame;
    
    public ResizeBasedChangeBase(int frame)
    {
        this.frame = frame;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        _originalSize = target.Size;
        _originalHorAxisY = target.HorizontalSymmetryAxisY;
        _originalVerAxisX = target.VerticalSymmetryAxisX;
        return true;
    }
    
    /// <summary>
    /// Notice: this commits image changes, you won't have a chance to revert or set ignoreInUndo to true
    /// </summary>
    protected virtual void Resize(ChunkyImage img, Guid memberGuid, VecI size, VecI offset, Dictionary<Guid, CommittedChunkStorage> deletedChunksDict)
    {
        img.EnqueueResize(size);
        img.EnqueueClear();
        img.EnqueueDrawChunkyImage(offset, img);

        deletedChunksDict.Add(memberGuid, new CommittedChunkStorage(img, img.FindAffectedArea().Chunks));
        img.CommitChanges();
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Size = _originalSize;
        target.ForEveryMember((member) =>
        {
            if (member is RasterLayer layer)
            {
                var layerImage = layer.GetLayerImageAtFrame(frame);
                layerImage.EnqueueResize(_originalSize);
                deletedChunks[layer.GuidValue].ApplyChunksToImage(layerImage);
                layerImage.CommitChanges();
            }

            // TODO: Add support for different Layer types?
            
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
