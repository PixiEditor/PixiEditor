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

    protected override Image? OnExecute(KeyFrameTime frame)
    {
        if (!IsVisible.Value || Opacity.Value <= 0)
        {
            Output.Value = Background.Value;
            return Output.Value;
        }

        var frameImage = GetFrameImage(frame).Image;

        Surface workingSurface;
        
        blendPaint.Color = new Color(255, 255, 255, (byte)Math.Round(Opacity.Value * 255));
        blendPaint.BlendMode = DrawingApi.Core.Surface.BlendMode.Src;

        if (Background.Value != null)
        {
            VecI targetSize = GetBiggerSize(frameImage.LatestSize, Background.Value.Size);
            workingSurface = new Surface(targetSize);
            workingSurface.DrawingSurface.Canvas.DrawImage(Background.Value, 0, 0, blendPaint);
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            
            using ChunkyImage img = new ChunkyImage(targetSize);
            img.EnqueueDrawUpToDateChunkyImage(VecI.Zero, frameImage);

            ApplyMaskIfPresent(img);
            img.CommitChanges();

            img.DrawMostUpToDateRegionOn(
                new RectI(0, 0, frameImage.LatestSize.X, frameImage.LatestSize.Y), 
                ChunkResolution.Full, workingSurface.DrawingSurface, VecI.Zero, blendPaint);
        }
        else
        {
            workingSurface = new Surface(frameImage.LatestSize);
            
            using ChunkyImage img = new ChunkyImage(frameImage.LatestSize);
            img.EnqueueDrawUpToDateChunkyImage(VecI.Zero, frameImage);
            
            ApplyMaskIfPresent(img);
            img.CommitChanges();
            
            img.DrawMostUpToDateRegionOn(
                new RectI(0, 0, frameImage.LatestSize.X, frameImage.LatestSize.Y), 
                ChunkResolution.Full, workingSurface.DrawingSurface, VecI.Zero, blendPaint);
        }


        Output.Value = workingSurface.DrawingSurface.Snapshot();
        
        workingSurface.Dispose();
        return Output.Value;
    }

    private ImageFrame GetFrameImage(KeyFrameTime frame)
    {
        var imageFrame = frames.FirstOrDefault(x => x.IsInFrame(frame.Frame));
        var frameImage = imageFrame ?? frames[0];
        return frameImage;
    }

    private void ApplyMaskIfPresent(ChunkyImage img)
    {
        if (Mask.Value != null)
        {
            img.EnqueueApplyMask(Mask.Value);
        }
    }

    protected override bool CacheChanged(KeyFrameTime frameTime)
    {
        var frame = GetFrameImage(frameTime);
        return base.CacheChanged(frameTime) || frame?.RequiresUpdate == true;
    }

    protected override void UpdateCache(KeyFrameTime time)
    {
        base.UpdateCache(time);
        var imageFrame = GetFrameImage(time);
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

    private VecI GetBiggerSize(VecI size1, VecI size2)
    {
        return new VecI(Math.Max(size1.X, size2.X), Math.Max(size1.Y, size2.Y));
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
    
    public bool RequiresUpdate
    {
        get
        {
            return Image.QueueLength != lastQueueLength;
        }
        set
        {
            lastQueueLength = Image.QueueLength;
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
