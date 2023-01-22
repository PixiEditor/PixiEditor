using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal sealed class RotateImage_Change : Change
{
    private readonly RotationAngle rotation;
    private List<Guid> membersToRotate;
    
    private VecI originalSize;
    private int originalHorAxisY;
    private int originalVerAxisX;
    private Dictionary<Guid, CommittedChunkStorage> deletedChunks = new();
    private Dictionary<Guid, CommittedChunkStorage> deletedMaskChunks = new();

    [GenerateMakeChangeAction]
    public RotateImage_Change(RotationAngle rotation, List<Guid>? membersToRotate)
    {
        this.rotation = rotation;
        membersToRotate ??= new List<Guid>();
        this.membersToRotate = membersToRotate;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (membersToRotate.Count > 0)
        {
            membersToRotate = target.ExtractLayers(membersToRotate);
            
            foreach (var layer in membersToRotate)
            {
                if (!target.HasMember(layer)) return false;
            }  
        }
        
        originalSize = target.Size;
        originalHorAxisY = target.HorizontalSymmetryAxisY;
        originalVerAxisX = target.VerticalSymmetryAxisX;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var changes = Rotate(target);
        
        ignoreInUndo = false;
        return changes;
    }

    private void Resize(ChunkyImage img, Guid memberGuid,
        Dictionary<Guid, CommittedChunkStorage> deletedChunksDict, List<IChangeInfo>? changes)
    {
        RectI bounds = new RectI(VecI.Zero, img.CommittedSize);
        if (membersToRotate.Count > 0)
        {
            var preciseBounds = img.FindPreciseCommittedBounds();
            if (preciseBounds.HasValue)
            {
                bounds = preciseBounds.Value;
            }
        }

        int originalWidth = bounds.Width;
        int originalHeight = bounds.Height;
        
        int newWidth = rotation == RotationAngle.D180 ? originalWidth : originalHeight;
        int newHeight = rotation == RotationAngle.D180 ? originalHeight : originalWidth;

        VecI oldSize = new VecI(originalWidth, originalHeight);
        VecI newSize = new VecI(newWidth, newHeight);
        
        using Paint paint = new()
        {
            BlendMode = DrawingApi.Core.Surface.BlendMode.Src
        };
        
        using Surface originalSurface = new(oldSize);
        img.DrawMostUpToDateRegionOn(
            bounds, 
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);

        using Surface flipped = new Surface(newSize);

        float translationX = newSize.X;
        float translationY = newSize.Y;
        switch (rotation)
        {
            case RotationAngle.D90:
                translationY = 0;
                break;
            case RotationAngle.D270:
                translationX = 0;
                break;
        }
        
        flipped.DrawingSurface.Canvas.Save();
        flipped.DrawingSurface.Canvas.Translate(translationX, translationY);
        flipped.DrawingSurface.Canvas.RotateRadians(RotationAngleToRadians(rotation), 0, 0);
        flipped.DrawingSurface.Canvas.DrawSurface(originalSurface.DrawingSurface, 0, 0, paint);
        flipped.DrawingSurface.Canvas.Restore();

        if (membersToRotate.Count == 0) 
        {
            img.EnqueueResize(newSize);
        }

        img.EnqueueClear();
        img.EnqueueDrawImage(bounds.Pos, flipped);

        var affArea = img.FindAffectedArea();
        deletedChunksDict.Add(memberGuid, new CommittedChunkStorage(img, affArea.Chunks));
        changes?.Add(new LayerImageArea_ChangeInfo(memberGuid, affArea));
        img.CommitChanges();
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> Rotate(Document target)
    {
        if (membersToRotate.Count == 0)
        {
            return RotateWholeImage(target);
        }

        return RotateMembers(target, membersToRotate);
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> RotateMembers(Document target, List<Guid> guids)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();

        target.ForEveryMember((member) =>
        {
            if (guids.Contains(member.GuidValue))
            {
                if (member is Layer layer)
                {
                    Resize(layer.LayerImage, layer.GuidValue, deletedChunks, changes);
                }

                if (member.Mask is null)
                    return;

                Resize(member.Mask, member.GuidValue, deletedMaskChunks, null);
            }
        });

        return changes;
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> RotateWholeImage(Document target)
    {
        int newWidth = rotation == RotationAngle.D180 ? target.Size.X : target.Size.Y;
        int newHeight = rotation == RotationAngle.D180 ? target.Size.Y : target.Size.X;

        VecI newSize = new VecI(newWidth, newHeight);

        float normalizedSymmX = originalVerAxisX / Math.Max(target.Size.X, 0.1f);
        float normalizedSymmY = originalHorAxisY / Math.Max(target.Size.Y, 0.1f);

        target.Size = newSize;
        target.VerticalSymmetryAxisX = (int)(newSize.X * normalizedSymmX);
        target.HorizontalSymmetryAxisY = (int)(newSize.Y * normalizedSymmY);

        target.ForEveryMember((member) =>
        {
            if (member is Layer layer)
            {
                Resize(layer.LayerImage, layer.GuidValue, deletedChunks, null);
            }

            if (member.Mask is null)
                return;

            Resize(member.Mask, member.GuidValue, deletedMaskChunks, null);
        });

        return new Size_ChangeInfo(newSize, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (membersToRotate.Count == 0)
        {
            return RevertRotateWholeImage(target);
        }

        return RevertRotateMembers(target);
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> RevertRotateWholeImage(Document target)
    {
        target.Size = originalSize;
        RevertRotateMembers(target);

        target.HorizontalSymmetryAxisY = originalHorAxisY;
        target.VerticalSymmetryAxisX = originalVerAxisX;

        return new Size_ChangeInfo(originalSize, originalVerAxisX, originalHorAxisY);
    }

    private List<IChangeInfo> RevertRotateMembers(Document target)
    {
        List<IChangeInfo> revertChanges = new List<IChangeInfo>();
        target.ForEveryMember((member) =>
        {
            if(membersToRotate.Count > 0 && !membersToRotate.Contains(member.GuidValue)) return;
            if (member is Layer layer)
            {
                layer.LayerImage.EnqueueResize(originalSize);
                deletedChunks[layer.GuidValue].ApplyChunksToImage(layer.LayerImage);
                revertChanges.Add(new LayerImageArea_ChangeInfo(layer.GuidValue, layer.LayerImage.FindAffectedArea()));
                layer.LayerImage.CommitChanges();
            }

            if (member.Mask is null)
                return;
            member.Mask.EnqueueResize(originalSize);
            deletedMaskChunks[member.GuidValue].ApplyChunksToImage(member.Mask);
            revertChanges.Add(new LayerImageArea_ChangeInfo(member.GuidValue, member.Mask.FindAffectedArea()));
            member.Mask.CommitChanges();
        });

        DisposeDeletedChunks();
        return revertChanges;
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

    private float RotationAngleToRadians(RotationAngle rotationAngle)
    {
        return rotationAngle switch
        {
            RotationAngle.D90 => 90f * Matrix3X3.DegreesToRadians,
            RotationAngle.D180 => 180f * Matrix3X3.DegreesToRadians,
            RotationAngle.D270 => 270f * Matrix3X3.DegreesToRadians,
            _ => throw new ArgumentOutOfRangeException(nameof(rotationAngle), rotationAngle, null)
        };
    }
}
