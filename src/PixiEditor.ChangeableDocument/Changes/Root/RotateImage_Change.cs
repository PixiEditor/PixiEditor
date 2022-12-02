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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        
    }

    private void Resize(ChunkyImage img, Guid memberGuid, Dictionary<Guid, CommittedChunkStorage> deletedChunksDict)
    {
        RectI bounds = new RectI(VecI.Zero, img.LatestSize);
        if (membersToRotate.Count > 0)
        {
            var preciseBounds = img.FindPreciseCommittedBounds();
            if (preciseBounds.HasValue)
            {
                bounds = preciseBounds.Value;
            }
        }
        
        int newWidth = rotation == RotationAngle.D180 ? bounds.Size.X : bounds.Size.Y;
        int newHeight = rotation == RotationAngle.D180 ? bounds.Size.Y : bounds.Size.X;
        
        VecI size = new VecI(newWidth, newHeight)''
        
        using Paint paint = new()
        {
            BlendMode = DrawingApi.Core.Surface.BlendMode.Src
        };
        
        using Surface originalSurface = new(img.LatestSize);
        img.DrawMostUpToDateRegionOn(
            new RectI(VecI.Zero, img.LatestSize), 
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);

        using Surface flipped = new Surface(img.LatestSize);

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
                int newWidth;
                int newHeight;

                if (member is Layer layer)
                {
                    Resize(layer.LayerImage, layer.GuidValue,
                            deletedChunks);
                    changes.Add(
                            new LayerImageChunks_ChangeInfo(member.GuidValue, layer.LayerImage.FindAffectedChunks()));
                    layer.LayerImage.CommitChanges();
                }

                if (member.Mask is null)
                    return;

                var maskBounds = member.Mask.FindPreciseCommittedBounds();
                if (maskBounds.HasValue)
                {
                    newWidth = rotation == RotationAngle.D180 ? maskBounds.Value.Size.X : maskBounds.Value.Size.Y;
                    newHeight = rotation == RotationAngle.D180 ? maskBounds.Value.Size.Y : maskBounds.Value.Size.X;

                    Resize(member.Mask, member.GuidValue, new VecI(newWidth, newHeight), VecI.Zero, deletedMaskChunks);
                    changes.Add(
                        new LayerImageChunks_ChangeInfo(member.GuidValue, member.Mask.FindAffectedChunks()));
                    member.Mask.CommitChanges();
                }
            }
        });

        return changes;
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> RotateWholeImage(Document target)
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
                layer.LayerImage.CommitChanges();
            }

            if (member.Mask is null)
                return;

            Resize(member.Mask, member.GuidValue, newSize, VecI.Zero, deletedMaskChunks);
            member.Mask.CommitChanges();
        });

        return new Size_ChangeInfo(newSize, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
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
