using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal sealed class RotateImage_Change : ResizeBasedChangeBase
{
    private readonly RotationAngle rotation;

    [GenerateMakeChangeAction]
    public RotateImage_Change(RotationAngle rotation)
    {
        this.rotation = rotation;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var changes = Rotate(target);
        
        ignoreInUndo = false;
        return changes;
    }

    protected override void Resize(ChunkyImage img, Guid memberGuid, VecI size, VecI offset, Dictionary<Guid, CommittedChunkStorage> deletedChunksDict)
    {
        using Paint paint = new()
        {
            BlendMode = DrawingApi.Core.Surface.BlendMode.Src
        };
        
        using Surface originalSurface = new(_originalSize);
        img.DrawMostUpToDateRegionOn(
            new RectI(VecI.Zero, _originalSize), 
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);

        using Surface flipped = new Surface(size);

        float translationX = size.X;
        float translationY = size.Y;
        if (rotation == RotationAngle.D90)
        {
            translationY = 0;
        }
        else if (rotation == RotationAngle.D270)
        {
            translationX = 0;
        }
        
        flipped.DrawingSurface.Canvas.Save();
        flipped.DrawingSurface.Canvas.Translate(translationX, translationY);
        flipped.DrawingSurface.Canvas.RotateRadians(RotationAngleToRadians(rotation), 0, 0);
        flipped.DrawingSurface.Canvas.DrawSurface(originalSurface.DrawingSurface, 0, 0, paint);
        flipped.DrawingSurface.Canvas.Restore();
        
        img.EnqueueResize(size);
        img.EnqueueClear();
        img.EnqueueDrawImage(VecI.Zero, flipped);

        deletedChunksDict.Add(memberGuid, new CommittedChunkStorage(img, img.FindAffectedChunks()));
        img.CommitChanges();
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
    
    private OneOf<None, IChangeInfo, List<IChangeInfo>> Rotate(Document target)
    {
        int newWidth = rotation == RotationAngle.D180 ? target.Size.X : target.Size.Y;
        int newHeight = rotation == RotationAngle.D180 ? target.Size.Y : target.Size.X;

        VecI newSize = new VecI(newWidth, newHeight);

        float normalizedSymmX = _originalVerAxisX / Math.Max(target.Size.X, 0.1f);
        float normalizedSymmY = _originalHorAxisY / Math.Max(target.Size.Y, 0.1f);
        
        target.Size = newSize;
        target.VerticalSymmetryAxisX = (int)(newSize.X * normalizedSymmX);
        target.HorizontalSymmetryAxisY = (int)(newSize.Y * normalizedSymmY);
        
        target.ForEveryMember((member) =>
        {
            if (member is Layer layer)
            {
                Resize(layer.LayerImage, layer.GuidValue, newSize, VecI.Zero, deletedChunks);
            }
            if (member.Mask is null)
                return;

            Resize(member.Mask, member.GuidValue, newSize, VecI.Zero, deletedMaskChunks);
        });
        
        return new Size_ChangeInfo(newSize, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }
}
