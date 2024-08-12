using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ImageLayer")]
public class ImageLayerNode : LayerNode, IReadOnlyImageNode
{
    public const string ImageFramesKey = "Frames";
    public const string ImageLayerKey = "LayerImage";

    public OutputProperty<Texture> RawOutput { get; }

    public InputProperty<bool> LockTransparency { get; }

    private VecI size;
    private ChunkyImage layerImage => keyFrames[0]?.Data as ChunkyImage;

    private static readonly Paint clearPaint = new()
    {
        BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src,
        Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent
    };

    // Handled by overriden CacheChanged
    protected override bool AffectedByAnimation => true;

    protected override bool AffectedByChunkResolution => true;

    protected override bool AffectedByChunkToUpdate => true;

    public ImageLayerNode(VecI size)
    {
        RawOutput = CreateOutput<Texture>(nameof(RawOutput), "RAW_LAYER_OUTPUT", null);

        LockTransparency = CreateInput<bool>("LockTransparency", "LOCK_TRANSPARENCY", false);

        if (keyFrames.Count == 0)
        {
            keyFrames.Add(new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = new ChunkyImage(size) });
        }

        this.size = size;
    }

    public override RectI? GetTightBounds(KeyFrameTime frameTime)
    {
        return GetLayerImageAtFrame(frameTime.Frame).FindTightCommittedBounds();
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return Output.Value;
        }

        var frameImage = GetFrameWithImage(context.FrameTime);

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

        var renderedSurface = RenderImage(frameImage.Data as ChunkyImage, context);

        Output.Value = renderedSurface;

        return Output.Value;
    }

    private Texture RenderImage(ChunkyImage frameImage, RenderingContext context)
    {
        var outputWorkingSurface = TryInitWorkingSurface(frameImage.LatestSize, context, 0);
        var filterlessWorkingSurface = TryInitWorkingSurface(frameImage.LatestSize, context, 1);
        var rawWorkingSurface = TryInitWorkingSurface(frameImage.LatestSize, context, 3);

        bool shouldClear = Background.Value == null;
        // Draw filterless
        if (Background.Value != null)
        {
            DrawBackground(filterlessWorkingSurface, context);
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
        }

        DrawLayer(frameImage, context, filterlessWorkingSurface, shouldClear, useFilters: false);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

        FilterlessOutput.Value = filterlessWorkingSurface;

        // Draw raw
        DrawLayer(frameImage, context, rawWorkingSurface, true, useFilters: false);

        RawOutput.Value = rawWorkingSurface;

        // Draw output
        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                DrawBackground(outputWorkingSurface, context);
                blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            }

            DrawLayer(frameImage, context, outputWorkingSurface, shouldClear);

            Output.Value = outputWorkingSurface;

            return outputWorkingSurface;
        }

        DrawLayer(frameImage, context, outputWorkingSurface, true);

        // shit gets downhill with mask on big canvases, TODO: optimize
        ApplyMaskIfPresent(outputWorkingSurface, context);
        ApplyRasterClip(outputWorkingSurface, context);

        if (Background.Value != null)
        {
            Texture tempSurface = new Texture(outputWorkingSurface.Size);
            DrawBackground(tempSurface, context);
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            tempSurface.DrawingSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);

            Output.Value = tempSurface;
            return tempSurface;
        }

        Output.Value = outputWorkingSurface;

        return outputWorkingSurface;
    }

    private void DrawLayer(ChunkyImage frameImage, RenderingContext context, Texture workingSurface, bool shouldClear,
        bool useFilters = true)
    {
        blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255));

        if (useFilters && Filters.Value != null)
        {
            DrawWithFilters(frameImage, context, workingSurface, shouldClear);
        }
        else
        {
            blendPaint.SetFilters(null);

            if (!frameImage.DrawMostUpToDateChunkOn(
                    context.ChunkToUpdate,
                    context.ChunkResolution,
                    workingSurface.DrawingSurface,
                    context.ChunkToUpdate * context.ChunkResolution.PixelSize(),
                    blendPaint) && shouldClear)
            {
                workingSurface.DrawingSurface.Canvas.DrawRect(CalculateDestinationRect(context), clearPaint);
            }
        }
    }

    // Draw with filters is a bit tricky since some filters sample data from chunks surrounding the chunk being drawn,
    // this is why we need to do intermediate drawing to a temporary surface and then apply filters to that surface
    private void DrawWithFilters(ChunkyImage frameImage, RenderingContext context, Texture workingSurface,
        bool shouldClear)
    {
        VecI imageChunksSize = frameImage.LatestSize / context.ChunkResolution.PixelSize();
        bool requiresTopLeft = context.ChunkToUpdate.X > 0 || context.ChunkToUpdate.Y > 0;
        bool requiresTop = context.ChunkToUpdate.Y > 0;
        bool requiresLeft = context.ChunkToUpdate.X > 0;
        bool requiresTopRight = context.ChunkToUpdate.X < imageChunksSize.X - 1 && context.ChunkToUpdate.Y > 0; 
        bool requiresRight = context.ChunkToUpdate.X < imageChunksSize.X - 1;
        bool requiresBottomRight = context.ChunkToUpdate.X < imageChunksSize.X - 1 && context.ChunkToUpdate.Y < imageChunksSize.Y - 1; 
        bool requiresBottom = context.ChunkToUpdate.Y < imageChunksSize.Y - 1;
        bool requiresBottomLeft = context.ChunkToUpdate.X > 0 && context.ChunkToUpdate.Y < imageChunksSize.Y - 1;

        VecI tempSizeInChunks = new VecI(1, 1);
        if(requiresLeft)
        {
            tempSizeInChunks.X++;
        }
        
        if(requiresRight)
        {
            tempSizeInChunks.X++;
        }
        
        if(requiresTop)
        {
            tempSizeInChunks.Y++;
        }
        
        if(requiresBottom)
        {
            tempSizeInChunks.Y++;
        }
        
        VecI tempSize = tempSizeInChunks * context.ChunkResolution.PixelSize();
        tempSize = new VecI(Math.Min(tempSize.X, workingSurface.Size.X), Math.Min(tempSize.Y, workingSurface.Size.Y));

        if (shouldClear)
        {
            workingSurface.DrawingSurface.Canvas.DrawRect(
                new RectI(
                    VecI.Zero,
                    tempSize), 
                clearPaint);
        }
        
        using Texture tempSurface = new Texture(tempSize);

        if (requiresTopLeft)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(-1, -1));
        }
        
        if (requiresTop)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(0, -1));
        }
        
        if (requiresLeft)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(-1, 0));
        }
        
        if (requiresTopRight)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(1, -1));
        }
        
        if (requiresRight)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(1, 0));
        }
        
        if (requiresBottomRight)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(1, 1));
        }
        
        if (requiresBottom)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(0, 1));
        }
        
        if (requiresBottomLeft)
        {
            DrawChunk(frameImage, context, tempSurface, new VecI(-1, 1));
        }
        
        DrawChunk(frameImage, context, tempSurface, new VecI(0, 0));
        
        blendPaint.SetFilters(Filters.Value);
        workingSurface.DrawingSurface.Canvas.DrawSurface(tempSurface.DrawingSurface, VecI.Zero, blendPaint);
    }

    private void DrawChunk(ChunkyImage frameImage, RenderingContext context, Texture tempSurface, VecI vecI)
    {
        VecI chunkPos = context.ChunkToUpdate + vecI;
        if (frameImage.LatestOrCommittedChunkExists(chunkPos))
        {
            frameImage.DrawMostUpToDateChunkOn(
                chunkPos,
                context.ChunkResolution,
                tempSurface.DrawingSurface,
                chunkPos * context.ChunkResolution.PixelSize(),
                blendPaint);
        }
    }

    private KeyFrameData GetFrameWithImage(KeyFrameTime frame)
    {
        var imageFrame = keyFrames.LastOrDefault(x => x.IsInFrame(frame.Frame));
        if (imageFrame?.Data is not ChunkyImage)
        {
            return keyFrames[0];
        }

        var frameImage = imageFrame;
        return frameImage;
    }

    protected override bool CacheChanged(RenderingContext context)
    {
        var frame = GetFrameWithImage(context.FrameTime);
        return base.CacheChanged(context) || frame?.RequiresUpdate == true;
    }

    protected override void UpdateCache(RenderingContext context)
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
        return new ImageLayerNode(size)
        {
            MemberName = MemberName,
            keyFrames = new List<KeyFrameData>()
            {
                // we are only copying the layer image, keyframes probably shouldn't be copied since they are controlled by AnimationData
                new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = layerImage.CloneFromCommitted() }
            }
        };
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

    bool ITransparencyLockable.LockTransparency
    {
        get => LockTransparency.Value; // TODO: I wonder if it should be NonOverridenValue
        set => LockTransparency.NonOverridenValue = value;
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
