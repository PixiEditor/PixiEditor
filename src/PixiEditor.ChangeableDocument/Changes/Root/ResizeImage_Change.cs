using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.Surface;
using PixiEditor.DrawingApi.Core.Surfaces.Surface.PaintImpl;
using PixiEditor.Numerics;
using BlendMode = PixiEditor.DrawingApi.Core.Surfaces.Surface.BlendMode;

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
        using Surface originalSurface = new(originalSize);
        image.DrawMostUpToDateRegionOn(
            new(VecI.Zero, originalSize),
            ChunkResolution.Full,
            originalSurface.DrawingSurface,
            VecI.Zero);

        bool downscaling = newSize.LengthSquared < originalSize.LengthSquared;
        FilterQuality quality = ToFilterQuality(method, downscaling);
        using Paint paint = new() { FilterQuality = quality, BlendMode = Enums.BlendMode.Src, };

        using Surface newSurface = new(newSize);
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
                layer.ForEveryFrame(img =>
                {
                    ScaleChunkyImage(img);
                    var affected = img.FindAffectedArea();
                    savedChunks[layer.Id] = new CommittedChunkStorage(img, affected.Chunks);
                    img.CommitChanges();
                });
            }

            // Add support for different Layer types

            if (member.Mask.Value is not null)
            {
                ScaleChunkyImage(member.Mask.Value);
                var affected = member.Mask.Value.FindAffectedArea();
                savedMaskChunks[member.Id] = new CommittedChunkStorage(member.Mask.Value, affected.Chunks);
                member.Mask.Value.CommitChanges();
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
                layer.ForEveryFrame(layerImage =>
                {
                    layerImage.EnqueueResize(originalSize);
                    layerImage.EnqueueClear();
                    savedChunks[layer.Id].ApplyChunksToImage(layerImage);
                    layerImage.CommitChanges();
                });
            }

            if (member.Mask.Value is not null)
            {
                member.Mask.Value.EnqueueResize(originalSize);
                member.Mask.Value.EnqueueClear();
                savedMaskChunks[member.Id].ApplyChunksToImage(member.Mask.Value);
                member.Mask.Value.CommitChanges();
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
