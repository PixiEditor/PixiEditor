using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using BlendMode = PixiEditor.DrawingApi.Core.Surface.BlendMode;

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
        using Paint paint = new()
        {
            FilterQuality = quality, 
            BlendMode = BlendMode.Src,
        };

        using Surface newSurface = new(newSize);
        newSurface.DrawingSurface.Canvas.Save();
        newSurface.DrawingSurface.Canvas.Scale(newSize.X / (float)originalSize.X, newSize.Y / (float)originalSize.Y);
        newSurface.DrawingSurface.Canvas.DrawSurface(originalSurface.DrawingSurface, 0, 0, paint);
        newSurface.DrawingSurface.Canvas.Restore();
        
        image.EnqueueResize(newSize);
        image.EnqueueClear();
        image.EnqueueDrawImage(VecI.Zero, newSurface);
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
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
            if (member is Layer layer)
            {
                ScaleChunkyImage(layer.LayerImage);
                var affected = layer.LayerImage.FindAffectedArea();
                savedChunks[layer.GuidValue] = new CommittedChunkStorage(layer.LayerImage, affected.Chunks);
                layer.LayerImage.CommitChanges();
            }
            if (member.Mask is not null)
            {
                ScaleChunkyImage(member.Mask);
                var affected = member.Mask.FindAffectedArea();
                savedMaskChunks[member.GuidValue] = new CommittedChunkStorage(member.Mask, affected.Chunks);
                member.Mask.CommitChanges();
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
            if (member is Layer layer)
            {
                layer.LayerImage.EnqueueResize(originalSize);
                layer.LayerImage.EnqueueClear();
                savedChunks[layer.GuidValue].ApplyChunksToImage(layer.LayerImage);
                layer.LayerImage.CommitChanges();
            }
            
            if (member.Mask is not null)
            {
                member.Mask.EnqueueResize(originalSize);
                member.Mask.EnqueueClear();
                savedMaskChunks[member.GuidValue].ApplyChunksToImage(member.Mask);
                member.Mask.CommitChanges();
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
