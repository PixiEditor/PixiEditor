using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal sealed class RotateImage_Change : Change
{
    private readonly RotationAngle rotation;
    private List<Guid> membersToRotate;

    private VecI originalSize;
    private double originalHorAxisY;
    private double originalVerAxisX;
    private Dictionary<Guid, CommittedChunkStorage> deletedChunks = new();
    private Dictionary<Guid, CommittedChunkStorage> deletedMaskChunks = new();
    private int? frame;

    [GenerateMakeChangeAction]
    public RotateImage_Change(RotationAngle rotation, List<Guid>? membersToRotate, int frame)
    {
        this.rotation = rotation;
        membersToRotate ??= new List<Guid>();
        this.membersToRotate = membersToRotate;
        this.frame = frame < 0 ? null : frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (membersToRotate.Count > 0)
        {
            membersToRotate = target.ExtractLayers(membersToRotate);

            RectD? bounds = null;
            foreach (var layer in membersToRotate)
            {
                if (!target.HasMember(layer)) return false;

                if (frame != null)
                {
                    var layerBounds = target.FindMember(layer).GetTightBounds(frame.Value);
                    if (layerBounds.HasValue)
                    {
                        bounds = bounds?.Union(layerBounds.Value) ?? layerBounds.Value;
                    }
                }
            }
            
            if(frame != null && (bounds == null || bounds.Value.IsZeroArea)) return false;
        }

        originalSize = target.Size;
        originalHorAxisY = target.HorizontalSymmetryAxisY;
        originalVerAxisX = target.VerticalSymmetryAxisX;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
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
            var preciseBounds = img.FindTightCommittedBounds();
            if (preciseBounds.HasValue)
            {
                bounds = preciseBounds.Value;
            }
        }
        
        if (bounds.IsZeroArea)
        {
            return;
        }

        int originalWidth = bounds.Width;
        int originalHeight = bounds.Height;

        int newWidth = rotation == RotationAngle.D180 ? originalWidth : originalHeight;
        int newHeight = rotation == RotationAngle.D180 ? originalHeight : originalWidth;

        VecI oldSize = new VecI(originalWidth, originalHeight);
        VecI newSize = new VecI(newWidth, newHeight);

        using Paint paint = new() { BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.Src };

        using Surface originalSurface = Surface.ForProcessing(oldSize, img.ProcessingColorSpace);
        img.DrawMostUpToDateRegionOn(
            bounds,
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);

        using Surface flipped = Surface.ForProcessing(newSize, img.ProcessingColorSpace);

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
            if (guids.Contains(member.Id))
            {
                if (member is ImageLayerNode layer)
                {
                    if (frame != null)
                    {
                        Resize(layer.GetLayerImageAtFrame(frame.Value), layer.Id, deletedChunks, changes);
                    }
                    else
                    {
                        layer.ForEveryFrame(img =>
                        {
                            Resize(img, layer.Id, deletedChunks, changes);
                        });
                    }
                }
                else if (member is ITransformableObject transformableObject)
                {
                    RectD? tightBounds = member.GetTightBounds(frame.Value);
                    transformableObject.TransformationMatrix = transformableObject.TransformationMatrix.PostConcat(
                        Matrix3X3.CreateRotation(
                            RotationAngleToRadians(rotation),
                            (float?)tightBounds?.Center.X ?? 0, (float?)tightBounds?.Center.Y ?? 0));
                }

                if (member.EmbeddedMask is null)
                    return;

                Resize(member.EmbeddedMask, member.Id, deletedMaskChunks, null);
            }
        });

        return changes;
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> RotateWholeImage(Document target)
    {
        int newWidth = rotation == RotationAngle.D180 ? target.Size.X : target.Size.Y;
        int newHeight = rotation == RotationAngle.D180 ? target.Size.Y : target.Size.X;

        VecI newSize = new VecI(newWidth, newHeight);

        double normalizedSymmX = originalVerAxisX / Math.Max(target.Size.X, 0.1f);
        double normalizedSymmY = originalHorAxisY / Math.Max(target.Size.Y, 0.1f);

        target.Size = newSize;
        target.VerticalSymmetryAxisX = Math.Round(newSize.X * normalizedSymmX * 2) / 2;
        target.HorizontalSymmetryAxisY = Math.Round(newSize.Y * normalizedSymmY * 2) / 2;

        target.ForEveryMember((member) =>
        {
            if (member is ImageLayerNode layer)
            {
                if (frame != null)
                {
                    Resize(layer.GetLayerImageAtFrame(frame.Value), layer.Id, deletedChunks, null);
                }
                else
                {
                    layer.ForEveryFrame(img =>
                    {
                        Resize(img, layer.Id, deletedChunks, null);
                    });
                }
            }

            if (member.EmbeddedMask is null)
                return;

            Resize(member.EmbeddedMask, member.Id, deletedMaskChunks, null);
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
            if (membersToRotate.Count > 0 && !membersToRotate.Contains(member.Id)) return;
            if (member is ImageLayerNode layer)
            {
                if (frame != null)
                {
                    var layerImage = layer.GetLayerImageAtFrame(frame.Value);
                    layerImage.EnqueueResize(originalSize);
                    deletedChunks[layer.Id].ApplyChunksToImage(layerImage);
                    revertChanges.Add(new LayerImageArea_ChangeInfo(layer.Id, layerImage.FindAffectedArea()));
                    layerImage.CommitChanges();
                }
                else
                {
                    layer.ForEveryFrame(img =>
                    {
                        img.EnqueueResize(originalSize);
                        deletedChunks[layer.Id].ApplyChunksToImage(img);
                        revertChanges.Add(new LayerImageArea_ChangeInfo(layer.Id, img.FindAffectedArea()));
                        img.CommitChanges();
                    });
                }
            }

            if (member.EmbeddedMask is null)
                return;
            member.EmbeddedMask.EnqueueResize(originalSize);
            deletedMaskChunks[member.Id].ApplyChunksToImage(member.EmbeddedMask);
            revertChanges.Add(new LayerImageArea_ChangeInfo(member.Id, member.EmbeddedMask.FindAffectedArea()));
            member.EmbeddedMask.CommitChanges();
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
