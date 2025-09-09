using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal abstract class ResizeBasedChangeBase : Change
{
    protected VecI _originalSize;
    protected double _originalHorAxisY;
    protected double _originalVerAxisX;
    protected Dictionary<Guid, List<CommittedChunkStorage>> deletedChunks = new();
    protected Dictionary<Guid, List<CommittedChunkStorage>> deletedMaskChunks = new();
    
    protected Dictionary<Guid, Matrix3X3> originalTransformations = new();

    public ResizeBasedChangeBase()
    {
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
    protected virtual void Resize(ChunkyImage img, Guid memberGuid, VecI size, VecI offset,
        Dictionary<Guid, List<CommittedChunkStorage>> deletedChunksDict)
    {
        img.EnqueueResize(size);
        img.EnqueueClear();
        img.EnqueueDrawCommitedChunkyImage(offset, img);
        
        if (!deletedChunksDict.ContainsKey(memberGuid))
            deletedChunksDict.Add(memberGuid, new());

        deletedChunksDict[memberGuid].Add(new CommittedChunkStorage(img, img.FindAffectedArea().Chunks));
        img.CommitChanges();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Size = _originalSize;
        target.ForEveryMember((member) =>
        {
            if (member is ImageLayerNode layer)
            {
                layer.ForEveryFrame((img, id) =>
                {
                    img.EnqueueResize(_originalSize);
                    foreach (var stored in deletedChunks[id])
                        stored.ApplyChunksToImage(img);
                    img.CommitChanges();
                });
            }
            else if (member is ITransformableObject transformableObject)
            {
                if (originalTransformations.TryGetValue(member.Id, out var transformation))
                {
                    transformableObject.TransformationMatrix = transformation;
                }
            }

            if (member.EmbeddedMask is null)
                return;
            member.EmbeddedMask.EnqueueResize(_originalSize);
            deletedMaskChunks[member.Id][0].ApplyChunksToImage(member.EmbeddedMask);
            member.EmbeddedMask.CommitChanges();
        });

        target.HorizontalSymmetryAxisY = _originalHorAxisY;
        target.VerticalSymmetryAxisX = _originalVerAxisX;

        DisposeDeletedChunks();

        return new Size_ChangeInfo(_originalSize, _originalVerAxisX, _originalHorAxisY);
    }

    private void DisposeDeletedChunks()
    {
        foreach (var stored in deletedChunks)
        {
            foreach (var storage in stored.Value)
            {
                storage.Dispose();
            }

        }
        deletedChunks = new();

        foreach (var stored in deletedMaskChunks)
        {
            foreach (var storage in stored.Value)
            {
                storage.Dispose();
            }
        }
        deletedMaskChunks = new();
    }

    public override void Dispose()
    {
        DisposeDeletedChunks();
    }
}
