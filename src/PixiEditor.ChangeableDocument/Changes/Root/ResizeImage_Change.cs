using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class ResizeImage_Change : Change
{
    private readonly VecI newSize;
    private readonly ResamplingMethod method;
    private VecI originalSize;
    private int originalHorAxisY;
    private int originalVerAxisX;
    
    private Dictionary<Guid, CommittedChunkStorage> savedChunks = new();
    private Dictionary<Guid, CommittedChunkStorage> savedMaskChunks = new();

    [GenerateMakeChangeAction]
    public ResizeImage_Change(VecI size, ResamplingMethod method)
    {
        this.newSize = size;
        this.method = method;
    }
    
    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.Size == newSize || newSize.X < 1 || newSize.Y < 1)
            return new Error();
        
        originalSize = target.Size;
        originalHorAxisY = target.HorizontalSymmetryAxisY;
        originalVerAxisX = target.VerticalSymmetryAxisX;
        return new Success();
    }

    private static SKFilterQuality ToFilterQuality(ResamplingMethod method, bool downscaling) =>
        (method, downscaling) switch
        {
            (ResamplingMethod.NearestNeighbor, _) => SKFilterQuality.None,
            (ResamplingMethod.Bilinear, true) => SKFilterQuality.Medium,
            (ResamplingMethod.Bilinear, false) => SKFilterQuality.Low,
            (ResamplingMethod.Bicubic, _) => SKFilterQuality.High,
            _ => throw new ArgumentOutOfRangeException(),
        };

    private void ScaleChunkyImage(ChunkyImage image)
    {
        using Surface originalSurface = new(originalSize);
        image.DrawMostUpToDateRegionOn(
            new(VecI.Zero, originalSize), 
            ChunkResolution.Full,
            originalSurface.SkiaSurface,
            VecI.Zero);
        
        bool downscaling = newSize.LengthSquared < originalSize.LengthSquared;
        SKFilterQuality quality = ToFilterQuality(method, downscaling);
        using SKPaint paint = new()
        {
            FilterQuality = quality, 
            BlendMode = SKBlendMode.Src,
        };

        using Surface newSurface = new(newSize);
        newSurface.SkiaSurface.Canvas.Save();
        newSurface.SkiaSurface.Canvas.Scale(newSize.X / (float)originalSize.X, newSize.Y / (float)originalSize.Y);
        newSurface.SkiaSurface.Canvas.DrawSurface(originalSurface.SkiaSurface, 0, 0, paint);
        newSurface.SkiaSurface.Canvas.Restore();
        
        image.EnqueueResize(newSize);
        image.EnqueueClear();
        image.EnqueueDrawImage(VecI.Zero, newSurface);
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.Size = newSize;
        target.VerticalSymmetryAxisX = Math.Clamp(originalVerAxisX, 0, target.Size.X);
        target.HorizontalSymmetryAxisY = Math.Clamp(originalHorAxisY, 0, target.Size.Y);

        target.ForEveryMember(member =>
        {
            if (member is Layer layer)
            {
                ScaleChunkyImage(layer.LayerImage);
                var affected = layer.LayerImage.FindAffectedChunks();
                savedChunks[layer.GuidValue] = new CommittedChunkStorage(layer.LayerImage, affected);
                layer.LayerImage.CommitChanges();
            }
            if (member.Mask is not null)
            {
                ScaleChunkyImage(member.Mask);
                var affected = member.Mask.FindAffectedChunks();
                savedMaskChunks[member.GuidValue] = new CommittedChunkStorage(member.Mask, affected);
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
