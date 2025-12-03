using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using BlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ResizeImage_Change : Change
{
    private readonly VecI newSize;
    private readonly ResamplingMethod method;
    private VecI originalSize;
    private double originalHorAxisY;
    private double originalVerAxisX;

    private Dictionary<Guid, CommittedChunkStorage> savedChunks = new();
    private Dictionary<Guid, CommittedChunkStorage> savedMaskChunks = new();

    [GenerateMakeChangeAction]
    public ResizeImage_Change(VecI size, ResamplingMethod method)
    {
        this.newSize = size;
        this.method = method;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (newSize.X < 1 || newSize.Y < 1)
            return false;

        originalSize = target.Size;
        originalHorAxisY = target.HorizontalSymmetryAxisY;
        originalVerAxisX = target.VerticalSymmetryAxisX;
        return true;
    }

    private static FilterQuality ToFilterQuality(ResamplingMethod method, bool downscaling) =>
        (method, downscaling) switch
        {
            (ResamplingMethod.NearestNeighbor, _) => FilterQuality.None,
            (ResamplingMethod.Bilinear, true) => FilterQuality.Medium,
            (ResamplingMethod.Bilinear, false) => FilterQuality.Low,
            (ResamplingMethod.Bicubic, _) => FilterQuality.High,
            _ => throw new ArgumentOutOfRangeException(),
        };

    private void ScaleChunkyImage(ChunkyImage image)
    {
        using Surface originalSurface = Surface.ForProcessing(originalSize, image.ProcessingColorSpace);
        image.DrawMostUpToDateRegionOn(
            new(VecI.Zero, originalSize),
            ChunkResolution.Full,
            originalSurface.DrawingSurface.Canvas,
            VecI.Zero);

        bool downscaling = newSize.LengthSquared < originalSize.LengthSquared;
        FilterQuality quality = ToFilterQuality(method, downscaling);
        using Paint paint = new() { FilterQuality = quality, BlendMode = BlendMode.Src, };

        using Surface newSurface = Surface.ForProcessing(newSize, image.ProcessingColorSpace);
        newSurface.DrawingSurface.Canvas.Save();
        newSurface.DrawingSurface.Canvas.Scale(newSize.X / (float)originalSize.X, newSize.Y / (float)originalSize.Y);
        newSurface.DrawingSurface.Canvas.DrawSurface(originalSurface.DrawingSurface, 0, 0, paint);
        newSurface.DrawingSurface.Canvas.Restore();

        image.EnqueueResize(newSize);
        image.EnqueueClear();
        image.EnqueueDrawImage(VecI.Zero, newSurface);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (originalSize == newSize)
        {
            ignoreInUndo = true;
            return new None();
        }

        target.Size = newSize;
        target.VerticalSymmetryAxisX = Math.Clamp(originalVerAxisX, 0, target.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(originalHorAxisY, 0, target.Size.Y);

        target.ForEveryMember(member =>
        {
            if (member is ImageLayerNode layer)
            {
                layer.ForEveryFrame((img, id) =>
                {
                    ScaleChunkyImage(img);
                    var affected = img.FindAffectedArea();
                    savedChunks[id] = new CommittedChunkStorage(img, affected.Chunks);
                    img.CommitChanges();
                });
            }
            else if (member is IScalable scalableLayer)
            {
                VecD multiplier = new VecD(newSize.X / (double)originalSize.X, newSize.Y / (double)originalSize.Y);
                scalableLayer.Resize(multiplier);
            }

            // Add support for different Layer types

            if (member.EmbeddedMask is not null)
            {
                ScaleChunkyImage(member.EmbeddedMask);
                var affected = member.EmbeddedMask.FindAffectedArea();
                savedMaskChunks[member.Id] = new CommittedChunkStorage(member.EmbeddedMask, affected.Chunks);
                member.EmbeddedMask.CommitChanges();
            }
        });

        ignoreInUndo = false;
        return new Size_ChangeInfo(newSize, target.VerticalSymmetryAxisX, target.HorizontalSymmetryAxisY);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Size = originalSize;
        target.ForEveryMember((member) =>
        {
            if (member is ImageLayerNode layer)
            {
                layer.ForEveryFrame((layerImage, id) =>
                {
                    layerImage.EnqueueResize(originalSize);
                    layerImage.EnqueueClear();
                    savedChunks[id].ApplyChunksToImage(layerImage);
                    layerImage.CommitChanges();
                });
            }
            else if (member is IScalable scalableLayer)
            {
                VecD multiplier = new VecD(originalSize.X / (double)newSize.X, originalSize.Y / (double)newSize.Y);
                scalableLayer.Resize(multiplier);
            }

            if (member.EmbeddedMask is not null)
            {
                member.EmbeddedMask.EnqueueResize(originalSize);
                member.EmbeddedMask.EnqueueClear();
                savedMaskChunks[member.Id].ApplyChunksToImage(member.EmbeddedMask);
                member.EmbeddedMask.CommitChanges();
            }
        });

        target.HorizontalSymmetryAxisY = originalHorAxisY;
        target.VerticalSymmetryAxisX = originalVerAxisX;

        foreach (var stored in savedChunks)
            stored.Value.Dispose();
        savedChunks = new();

        foreach (var stored in savedMaskChunks)
            stored.Value.Dispose();
        savedMaskChunks = new();

        return new Size_ChangeInfo(originalSize, originalVerAxisX, originalHorAxisY);
    }

    public override void Dispose()
    {
        foreach (var layer in savedChunks)
            layer.Value.Dispose();
        foreach (var mask in savedMaskChunks)
            mask.Value.Dispose();
    }
}
