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

    private List<ImageFrame> frames = new List<ImageFrame>();
    private VecI size;

    private Paint blendPaint = new Paint();
    private Paint maskPaint = new Paint() { BlendMode = DrawingApi.Core.Surface.BlendMode.DstIn };

    private Dictionary<ChunkResolution, Surface> workingSurfaces = new Dictionary<ChunkResolution, Surface>();

    // Handled by overriden CacheChanged
    protected override bool AffectedByAnimation => false;

    protected override bool AffectedByChunkResolution => true;

    protected override bool AffectedByChunkToUpdate => true;

    public ImageLayerNode(VecI size)
    {
        LockTransparency = CreateInput<bool>("LockTransparency", "LOCK_TRANSPARENCY", false);
        frames.Add(new ImageFrame(Guid.NewGuid(), 0, 0, new(size)));
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

        var frameImage = GetFrameImage(context.FrameTime).Image;

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
        frameImage.DrawMostUpToDateChunkOn(
            context.ChunkToUpdate,
            context.ChunkResolution,
            workingSurface.DrawingSurface,
            context.ChunkToUpdate * context.ChunkResolution.PixelSize(),
            blendPaint);
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
        var imageFrame = frames.FirstOrDefault(x => x.IsInFrame(frame.Frame));
        var frameImage = imageFrame ?? frames[0];
        return frameImage;
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

    public override void Dispose()
    {
        base.Dispose();
        foreach (var frame in frames)
        {
            frame.Image.Dispose();
        }
    }

    public override Node CreateCopy()
    {
        return new ImageLayerNode(size)
        {
            frames = frames.Select(x =>
                new ImageFrame(x.KeyFrameGuid, x.StartFrame, x.Duration, x.Image.CloneFromCommitted())).ToList(),
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

    public void SetKeyFrameLength(Guid keyFrameGuid, int startFrame, int duration)
    {
        ImageFrame frame = frames.FirstOrDefault(x => x.KeyFrameGuid == keyFrameGuid);
        if (frame is not null)
        {
            frame.StartFrame = startFrame;
            frame.Duration = duration;
        }
    }

    public void ForEveryFrame(Action<ChunkyImage> action)
    {
        foreach (var frame in frames)
        {
            action(frame.Image);
        }
    }

    public ChunkyImage GetLayerImageAtFrame(int frame)
    {
        return frames.FirstOrDefault(x => x.IsInFrame(frame))?.Image ?? frames[0].Image;
    }

    public ChunkyImage GetLayerImageByKeyFrameGuid(Guid keyFrameGuid)
    {
        return frames.FirstOrDefault(x => x.KeyFrameGuid == keyFrameGuid)?.Image ?? frames[0].Image;
    }

    public void SetLayerImageAtFrame(int frame, ChunkyImage newLayerImage)
    {
        ImageFrame existingFrame = frames.FirstOrDefault(x => x.IsInFrame(frame));
        if (existingFrame is not null)
        {
            existingFrame.Image.Dispose();
            existingFrame.Image = newLayerImage;
        }
    }
    /*
          /// <summary>
        /// Creates a clone of the layer, its image and its mask
        /// </summary>
        internal override RasterLayer Clone()
        {
            List<ImageFrame> clonedFrames = new();
            foreach (var frame in frameImages)
            {
                clonedFrames.Add(new ImageFrame(frame.KeyFrameGuid, frame.StartFrame, frame.Duration,
                    frame.Image.CloneFromCommitted()));
            }

            return new RasterLayer(clonedFrames)
            {
                GuidValue = GuidValue,
                IsVisible = IsVisible,
                Name = Name,
                Opacity = Opacity,
                Mask = Mask?.CloneFromCommitted(),
                ClipToMemberBelow = ClipToMemberBelow,
                MaskIsVisible = MaskIsVisible,
                BlendMode = BlendMode,
                LockTransparency = LockTransparency
            };
        }
        public override void RemoveKeyFrame(Guid keyFrameGuid)
        {
            // Remove all in case I'm the lucky winner of guid collision
            frameImages.RemoveAll(x => x.KeyFrameGuid == keyFrameGuid);
        }

        public void SetLayerImageAtFrame(int frame, ChunkyImage newLayerImage)
        {
            ImageFrame existingFrame = frameImages.FirstOrDefault(x => x.IsInFrame(frame));
            if (existingFrame is not null)
            {
                existingFrame.Image.Dispose();
                existingFrame.Image = newLayerImage;
            }
        }


        public void AddFrame(Guid keyFrameGuid, int startFrame, int duration, ChunkyImage frameImg)
        {
            ImageFrame newFrame = new(keyFrameGuid, startFrame, duration, frameImg);
            frames.Add(newFrame);
        }
        */
}

class ImageFrame
{
    public int StartFrame { get; set; }
    public int Duration { get; set; }
    public ChunkyImage Image { get; set; }

    private int lastCommitCounter = 0;

    public bool RequiresUpdate
    {
        get
        {
            return Image.QueueLength != lastQueueLength || Image.CommitCounter != lastCommitCounter;
        }
        set
        {
            lastQueueLength = Image.QueueLength;
            lastCommitCounter = Image.CommitCounter;
        }
    }

    public Guid KeyFrameGuid { get; set; }
    private int lastQueueLength = 0;

    public ImageFrame(Guid keyFrameGuid, int startFrame, int duration, ChunkyImage image)
    {
        StartFrame = startFrame;
        Duration = duration;
        Image = image;
        KeyFrameGuid = keyFrameGuid;
    }

    public bool IsInFrame(int frame)
    {
        return frame >= StartFrame && frame < StartFrame + Duration;
    }
}
