using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageLayerNode : LayerNode, IReadOnlyImageNode
{
    public InputProperty<bool> LockTransparency { get; }

    private VecI size;

    private Paint blendPaint = new Paint();
    private Paint maskPaint = new Paint() { BlendMode = DrawingApi.Core.Surface.BlendMode.DstIn };
    private static readonly Paint clearPaint = new() { BlendMode = DrawingApi.Core.Surface.BlendMode.Src, 
        Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent };

    private Dictionary<ChunkResolution, Surface> workingSurfaces = new Dictionary<ChunkResolution, Surface>();

    // Handled by overriden CacheChanged
    protected override bool AffectedByAnimation => true;

    protected override bool AffectedByChunkResolution => true;

    protected override bool AffectedByChunkToUpdate => true;

    public ImageLayerNode(VecI size)
    {
        LockTransparency = CreateInput<bool>("LockTransparency", "LOCK_TRANSPARENCY", false);
        keyFrames.Add(new ImageFrame(Guid.NewGuid(), 0, 0, new(size)));
        this.size = size;
    }

    public override RectI? GetTightBounds(KeyFrameTime frameTime)
    {
        return GetLayerImageAtFrame(frameTime.Frame).FindTightCommittedBounds();
    }

    public override bool Validate()
    {
        return true;
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return Output.Value;
        }

        var frameImage = GetFrameImage(context.FrameTime).Data;

        blendPaint.Color = new Color(255, 255, 255, (byte)Math.Round(Opacity.Value * 255));
        blendPaint.BlendMode = DrawingApi.Core.Surface.BlendMode.Src;

        var renderedSurface = RenderImage(frameImage, context);

        Output.Value = renderedSurface;

        return Output.Value;
    }

    private Surface RenderImage(ChunkyImage frameImage, RenderingContext context)
    {
        ChunkResolution targetResolution = context.ChunkResolution;
        bool hasSurface = workingSurfaces.TryGetValue(targetResolution, out Surface workingSurface);
        VecI targetSize = (VecI)(frameImage.LatestSize * targetResolution.Multiplier());

        if (!hasSurface || workingSurface.Size != targetSize || workingSurface.IsDisposed)
        {
            workingSurfaces[targetResolution] = new Surface(targetSize);
            workingSurface = workingSurfaces[targetResolution];
        }

        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                DrawBackground(workingSurface, context);
                blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            }

            DrawLayer(frameImage, context, workingSurface);
            Output.Value = workingSurface;
            return workingSurface;
        }

        DrawLayer(frameImage, context, workingSurface);

        // shit gets downhill with mask on big canvases, TODO: optimize
        ApplyMaskIfPresent(workingSurface, context);
        ApplyRasterClip(workingSurface, context);

        if (Background.Value != null)
        {
            Surface tempSurface = new Surface(workingSurface.Size);
            DrawBackground(tempSurface, context);
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            tempSurface.DrawingSurface.Canvas.DrawSurface(workingSurface.DrawingSurface, 0, 0, blendPaint);

            Output.Value = tempSurface;
            return tempSurface;
        }

        Output.Value = workingSurface;
        return workingSurface;
    }

    private void DrawBackground(Surface workingSurface, RenderingContext context)
    {
        RectI source = CalculateSourceRect(Background.Value, workingSurface.Size, context);
        RectI target = CalculateDestinationRect(context);
        using var snapshot = Background.Value.DrawingSurface.Snapshot(source);

        workingSurface.DrawingSurface.Canvas.DrawImage(snapshot, target.X, target.Y, blendPaint);
    }

    private void DrawLayer(ChunkyImage frameImage, RenderingContext context, Surface workingSurface)
    {
        if (!frameImage.DrawMostUpToDateChunkOn(
                context.ChunkToUpdate,
                context.ChunkResolution,
                workingSurface.DrawingSurface,
                context.ChunkToUpdate * context.ChunkResolution.PixelSize(),
                blendPaint))
        {
            workingSurface.DrawingSurface.Canvas.DrawRect(CalculateDestinationRect(context), clearPaint);
        }
    }

    private bool IsEmptyMask()
    {
        return Mask.Value != null && MaskIsVisible.Value && !Mask.Value.LatestOrCommittedChunkExists();
    }

    private bool HasOperations()
    {
        return (MaskIsVisible.Value && Mask.Value != null) || ClipToPreviousMember.Value;
    }

    private void ApplyRasterClip(Surface surface, RenderingContext context)
    {
        if (ClipToPreviousMember.Value)
        {
            RectI? clippingRect = null;
            VecI chunkStart = context.ChunkToUpdate * context.ChunkResolution.PixelSize();
            VecI size = new VecI(context.ChunkResolution.PixelSize());
            clippingRect = new RectI(chunkStart, size);

            OperationHelper.ClampAlpha(surface.DrawingSurface, Background.Value, clippingRect);
        }
    }

    private ImageFrame GetFrameImage(KeyFrameTime frame)
    {
        var imageFrame = keyFrames.LastOrDefault(x => x.IsInFrame(frame.Frame));
        if (imageFrame is not ImageFrame)
        {
            return keyFrames[0] as ImageFrame;
        }
        
        var frameImage = imageFrame ?? keyFrames[0];
        return frameImage as ImageFrame;
    }

    private void ApplyMaskIfPresent(Surface surface, RenderingContext context)
    {
        if (Mask.Value != null && MaskIsVisible.Value)
        {
            Mask.Value.DrawMostUpToDateChunkOn(
                context.ChunkToUpdate,
                context.ChunkResolution,
                surface.DrawingSurface,
                context.ChunkToUpdate * context.ChunkResolution.PixelSize(),
                maskPaint);
        }
    }

    private RectI CalculateSourceRect(Surface image, VecI targetSize, RenderingContext context)
    {
        float multiplierToFit = image.Size.X / (float)targetSize.X;
        int chunkSize = context.ChunkResolution.PixelSize();
        VecI chunkPos = context.ChunkToUpdate;

        int x = (int)(chunkPos.X * chunkSize * multiplierToFit);
        int y = (int)(chunkPos.Y * chunkSize * multiplierToFit);
        int width = (int)(chunkSize * multiplierToFit);
        int height = (int)(chunkSize * multiplierToFit);

        return new RectI(x, y, width, height);
    }

    private RectI CalculateDestinationRect(RenderingContext context)
    {
        int chunkSize = context.ChunkResolution.PixelSize();
        VecI chunkPos = context.ChunkToUpdate;

        int x = chunkPos.X * chunkSize;
        int y = chunkPos.Y * chunkSize;
        int width = chunkSize;
        int height = chunkSize;

        return new RectI(x, y, width, height);
    }

    protected override bool CacheChanged(RenderingContext context)
    {
        var frame = GetFrameImage(context.FrameTime);
        return base.CacheChanged(context) || frame?.RequiresUpdate == true;
    }

    protected override void UpdateCache(RenderingContext context)
    {
        base.UpdateCache(context);
        var imageFrame = GetFrameImage(context.FrameTime);
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
        };
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
            if (frame is ImageFrame imageFrame)
            {
                action(imageFrame.Data);
            }
        }
    }

    public ChunkyImage GetLayerImageAtFrame(int frame)
    {
        return GetFrameImage(frame).Data;
    }

    public ChunkyImage GetLayerImageByKeyFrameGuid(Guid keyFrameGuid)
    {
        foreach (var keyFrame in keyFrames)
        {
            if (keyFrame.KeyFrameGuid == keyFrameGuid)
            {
                return (keyFrame as ImageFrame).Data;
            }
        }

        return (keyFrames[0] as ImageFrame).Data;        
    }

    public void SetLayerImageAtFrame(int frame, ChunkyImage newLayerImage)
    {
        var existingFrame = keyFrames.FirstOrDefault(x => x.IsInFrame(frame));
        if (existingFrame is not null && existingFrame is ImageFrame imgFrame)
        {
            existingFrame.Dispose();
            imgFrame.Data = newLayerImage;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        blendPaint.Dispose();
        maskPaint.Dispose();
        clearPaint.Dispose();
        foreach (var surface in workingSurfaces.Values)
        {
            surface.Dispose();
        }
    }
}

class ImageFrame : KeyFrameData<ChunkyImage>
{
    private int lastCommitCounter = 0;

    public override bool RequiresUpdate
    {
        get
        {
            return Data.QueueLength != lastQueueLength || Data.CommitCounter != lastCommitCounter;
        }
        set
        {
            lastQueueLength = Data.QueueLength;
            lastCommitCounter = Data.CommitCounter;
        }
    }

    private int lastQueueLength = 0;

    public ImageFrame(Guid keyFrameGuid, int startFrame, int duration, ChunkyImage image) : base(keyFrameGuid, image, startFrame, duration)
    {
    }
}
