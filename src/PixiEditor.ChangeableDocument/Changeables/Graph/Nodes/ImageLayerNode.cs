using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ImageLayer")]
public class ImageLayerNode : LayerNode, IReadOnlyImageNode
{
    public const string ImageFramesKey = "Frames";
    public const string ImageLayerKey = "LayerImage";
    public OutputProperty<Texture> RawOutput { get; }

    public bool LockTransparency { get; set; }

    private VecI size;
    private ChunkyImage layerImage => keyFrames[0]?.Data as ChunkyImage;
    protected Paint replacePaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src };

    private static readonly Paint clearPaint = new()
    {
        BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src,
        Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent
    };

    // Handled by overriden CacheChanged
    protected override bool AffectedByAnimation => true;

    protected override bool AffectedByChunkResolution => true;


    private Dictionary<ChunkResolution, Texture> renderedSurfaces = new();

    public ImageLayerNode(VecI size)
    {
        RawOutput = CreateOutput<Texture>(nameof(RawOutput), "RAW_LAYER_OUTPUT", null);

        if (keyFrames.Count == 0)
        {
            keyFrames.Add(new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = new ChunkyImage(size) });
        }

        this.size = size;
        
        renderedSurfaces[ChunkResolution.Full] = new Texture(size);
        renderedSurfaces[ChunkResolution.Half] = new Texture(new VecI(Math.Max(size.X / 2, 1), Math.Max(size.Y / 2, 1))); 
        renderedSurfaces[ChunkResolution.Quarter] = new Texture(new VecI(Math.Max(size.X / 4, 1), Math.Max(size.Y / 4, 1)));
        renderedSurfaces[ChunkResolution.Eighth] = new Texture(new VecI(Math.Max(size.X / 8, 1), Math.Max(size.Y / 8, 1)));
    }


    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return (RectD?)GetLayerImageAtFrame(frameTime.Frame).FindTightCommittedBounds();
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);

        /*if (RawOutput.Connections.Count > 0)
        {
            var rawWorkingSurface = TryInitWorkingSurface(GetTargetSize(context), context.ChunkResolution, 2);
            DrawLayer(context, rawWorkingSurface, true, useFilters: false);

            RawOutput.Value = rawWorkingSurface;
        }*/
    }

    public override VecD ScenePosition => VecD.Zero;
    public override VecD SceneSize => size;

    protected override VecI GetTargetSize(RenderContext ctx)
    {
        return (GetFrameWithImage(ctx.FrameTime).Data as ChunkyImage).LatestSize;
    }

    protected internal override void DrawLayer(SceneObjectRenderContext ctx, DrawingSurface workingSurface, bool shouldClear, bool useFilters = true)
    {
        int scaled = workingSurface.Canvas.Save();
        float multiplier = (float)ctx.ChunkResolution.InvertedMultiplier();
        workingSurface.Canvas.Scale(multiplier, multiplier);
        base.DrawLayer(ctx, workingSurface, shouldClear, useFilters);
        
        workingSurface.Canvas.RestoreToCount(scaled);
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        bool shouldClear,
        Paint paint)
    {
        workingSurface.Canvas.DrawSurface(renderedSurfaces[ctx.ChunkResolution].DrawingSurface, VecI.Zero, paint); 
    }

    // Draw with filters is a bit tricky since some filters sample data from chunks surrounding the chunk being drawn,
    // this is why we need to do intermediate drawing to a temporary surface and then apply filters to that surface
    protected override void DrawWithFilters(SceneObjectRenderContext context, DrawingSurface workingSurface,
        bool shouldClear, Paint paint)
    {
        // TODO: Implement non-chunk rendering
        /*var frameImage = GetFrameWithImage(context.FrameTime).Data as ChunkyImage;

        VecI chunkToUpdate = context.ChunkToUpdate.Value;

        VecI imageChunksSize = frameImage.LatestSize / context.ChunkResolution.PixelSize();
        bool requiresTopLeft = chunkToUpdate.X > 0 || chunkToUpdate.Y > 0;
        bool requiresTop = chunkToUpdate.Y > 0;
        bool requiresLeft = chunkToUpdate.X > 0;
        bool requiresTopRight = chunkToUpdate.X < imageChunksSize.X - 1 && chunkToUpdate.Y > 0;
        bool requiresRight = chunkToUpdate.X < imageChunksSize.X - 1;
        bool requiresBottomRight = chunkToUpdate.X < imageChunksSize.X - 1 &&
                                   chunkToUpdate.Y < imageChunksSize.Y - 1;
        bool requiresBottom = chunkToUpdate.Y < imageChunksSize.Y - 1;
        bool requiresBottomLeft = chunkToUpdate.X > 0 && chunkToUpdate.Y < imageChunksSize.Y - 1;

        VecI tempSizeInChunks = new VecI(1, 1);
        if (requiresLeft)
        {
            tempSizeInChunks.X++;
        }

        if (requiresRight)
        {
            tempSizeInChunks.X++;
        }

        if (requiresTop)
        {
            tempSizeInChunks.Y++;
        }

        if (requiresBottom)
        {
            tempSizeInChunks.Y++;
        }

        VecI tempSize = tempSizeInChunks * context.ChunkResolution.PixelSize();
        tempSize = new VecI(Math.Min(tempSize.X, (int)context.LocalBounds.Size.X),
            Math.Min(tempSize.Y, (int)context.LocalBounds.Size.Y));

        if (shouldClear)
        {
            workingSurface.Canvas.DrawRect(
                new RectD(
                    VecI.Zero,
                    tempSize),
                clearPaint);
        }

        using Texture tempSurface = new Texture(tempSize);

        if (requiresTopLeft)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(-1, -1), paint);
        }

        if (requiresTop)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(0, -1), paint);
        }

        if (requiresLeft)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(-1, 0), paint);
        }

        if (requiresTopRight)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(1, -1), paint);
        }

        if (requiresRight)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(1, 0), paint);
        }

        if (requiresBottomRight)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(1, 1), paint);
        }

        if (requiresBottom)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(0, 1), paint);
        }

        if (requiresBottomLeft)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(-1, 1), paint);
        }

        DrawChunk(frameImage, context, tempSurface, new VecI(0, 0), paint);

        workingSurface.Canvas.DrawSurface(tempSurface.DrawingSurface, VecI.Zero, paint);*/
    }

    public override bool RenderPreview(Texture renderOn, VecI chunk, ChunkResolution resolution, int frame)
    {
        var img = GetLayerImageAtFrame(frame);

        if (img is null)
        {
            return false;
        }

        img.DrawMostUpToDateChunkOn(
            chunk,
            resolution,
            renderOn.DrawingSurface,
            chunk * resolution.PixelSize(),
            blendPaint);

        return true;
    }


    private void DrawChunk(ChunkyImage frameImage, RenderContext context, Texture tempSurface, VecI vecI,
        Paint paint)
    {
        /*VecI chunkPos = context.ChunkToUpdate.Value + vecI;
        if (frameImage.LatestOrCommittedChunkExists(chunkPos))
        {
            frameImage.DrawMostUpToDateChunkOn(
                chunkPos,
                context.ChunkResolution,
                tempSurface.DrawingSurface,
                chunkPos * context.ChunkResolution.PixelSize(),
                paint);
        }*/
    }

    private KeyFrameData GetFrameWithImage(KeyFrameTime frame)
    {
        var imageFrame = keyFrames.OrderBy(x => x.StartFrame).LastOrDefault(x => x.IsInFrame(frame.Frame));
        if (imageFrame?.Data is not ChunkyImage)
        {
            return keyFrames[0];
        }

        var frameImage = imageFrame;
        return frameImage;
    }

    protected override bool CacheChanged(RenderContext context)
    {
        var frame = GetFrameWithImage(context.FrameTime);

        return base.CacheChanged(context) || frame?.RequiresUpdate == true;
    }

    protected override void UpdateCache(RenderContext context)
    {
        base.UpdateCache(context);
        var imageFrame = GetFrameWithImage(context.FrameTime);
        if (imageFrame is not null && imageFrame.RequiresUpdate)
        {
            imageFrame.RequiresUpdate = false;
        }
    }

    public override Node CreateCopy()
    {
        var image = new ImageLayerNode(size) { MemberName = this.MemberName, };

        image.keyFrames.Clear();

        return image;
    }

    public override void Dispose()
    {
        base.Dispose();

        foreach (var workingSurface in workingSurfaces)
        {
            workingSurface.Value.Dispose();
        }
    }


    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageAtFrame(int frame) => GetLayerImageAtFrame(frame);

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageByKeyFrameGuid(Guid keyFrameGuid) =>
        GetLayerImageByKeyFrameGuid(keyFrameGuid);

    void IReadOnlyImageNode.SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage newLayerImage) =>
        SetLayerImageAtFrame(frame, (ChunkyImage)newLayerImage);

    void IReadOnlyImageNode.ForEveryFrame(Action<IReadOnlyChunkyImage> action) => ForEveryFrame(action);
    
    public void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        var img = GetLayerImageAtFrame(frameTime.Frame);

        if (img is null)
        {
            return;
        }

        img.DrawMostUpToDateChunkOn(
            chunkPos,
            resolution,
            renderedSurfaces[resolution].DrawingSurface,
            chunkPos * resolution.PixelSize(),
            replacePaint);
        
        renderedSurfaces[resolution].DrawingSurface.Flush();
    }

    public void ForEveryFrame(Action<ChunkyImage> action)
    {
        foreach (var frame in keyFrames)
        {
            if (frame.Data is ChunkyImage imageFrame)
            {
                action(imageFrame);
            }
        }
    }

    public ChunkyImage GetLayerImageAtFrame(int frame)
    {
        return GetFrameWithImage(frame).Data as ChunkyImage;
    }

    public ChunkyImage GetLayerImageByKeyFrameGuid(Guid keyFrameGuid)
    {
        foreach (var keyFrame in keyFrames)
        {
            if (keyFrame.KeyFrameGuid == keyFrameGuid)
            {
                return keyFrame.Data as ChunkyImage;
            }
        }

        return layerImage;
    }

    public void SetLayerImageAtFrame(int frame, ChunkyImage newLayerImage)
    {
        var existingFrame = keyFrames.FirstOrDefault(x => x.IsInFrame(frame));
        if (existingFrame is not null && existingFrame.Data is ChunkyImage)
        {
            existingFrame.Dispose();
            existingFrame.Data = newLayerImage;
        }
    }
}
