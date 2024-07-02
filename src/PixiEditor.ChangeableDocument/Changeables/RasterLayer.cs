using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class RasterLayer : Layer, IReadOnlyRasterLayer
{
    // Don't forget to update CreateLayer_ChangeInfo, DocumentUpdater.ProcessCreateStructureMember and Layer.Clone when adding new properties
    public bool LockTransparency { get; set; } = false;


    private List<ImageFrame> frameImages = new();

    public RasterLayer(VecI size)
    {
        frameImages.Add(new ImageFrame(Guid.NewGuid(), 0, 0, new(size)));
    }

    public RasterLayer(List<ImageFrame> frames)
    {
        frameImages = frames;
    }

    /// <summary>
    /// Disposes the layer's image and mask
    /// </summary>
    public override void Dispose()
    {
        Mask?.Dispose();
        foreach (var frame in frameImages)
        {
            frame.Image.Dispose();
        }
    }

    IReadOnlyChunkyImage IReadOnlyRasterLayer.GetLayerImageAtFrame(int frame) => GetLayerImageAtFrame(frame);
    IReadOnlyChunkyImage IReadOnlyRasterLayer.GetLayerImageByKeyFrameGuid(Guid keyFrameGuid) => GetLayerImageByKeyFrameGuid(keyFrameGuid);
    void IReadOnlyRasterLayer.SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage newLayerImage) => SetLayerImageAtFrame(frame, (ChunkyImage)newLayerImage);

    void IReadOnlyRasterLayer.ForEveryFrame(Action<IReadOnlyChunkyImage> action) => ForEveryFrame(action);

    public void ForEveryFrame(Action<ChunkyImage> action)
    {
        foreach (var frame in frameImages)
        {
            action(frame.Image);
        }
    }

    public ChunkyImage GetLayerImageAtFrame(int frame)
    {
        return Rasterize(frame);
    }
    
    public ChunkyImage GetLayerImageByKeyFrameGuid(Guid keyFrameGuid)
    {
        return frameImages.FirstOrDefault(x => x.KeyFrameGuid == keyFrameGuid)?.Image ?? frameImages[0].Image;
    }

    public override ChunkyImage Rasterize(KeyFrameTime frameTime)
    {
        if (frameTime.Frame == 0 || frameImages.Count == 1)
        {
            return frameImages[0].Image;
        }

        ImageFrame frame = frameImages.FirstOrDefault(x => x.IsInFrame(frameTime.Frame));

        return frame?.Image ?? frameImages[0].Image;
    }

    public override void RemoveKeyFrame(Guid keyFrameGuid)
    {
        // Remove all in case I'm the lucky winner of guid collision
        frameImages.RemoveAll(x => x.KeyFrameGuid == keyFrameGuid);
    }

    public override RectI? GetTightBounds(int frame)
    {
        return Rasterize(frame).FindTightCommittedBounds();
    }

    /// <summary>
    /// Creates a clone of the layer, its image and its mask
    /// </summary>
    internal override RasterLayer Clone()
    {
        List<ImageFrame> clonedFrames = new();
        foreach (var frame in frameImages)
        {
            clonedFrames.Add(new ImageFrame(frame.KeyFrameGuid, frame.StartFrame, frame.Duration, frame.Image.CloneFromCommitted()));
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
        frameImages.Add(newFrame);
    }
}

class ImageFrame
{
    public int StartFrame { get; set; }
    public int Duration { get; set; }
    public ChunkyImage Image { get; set; }
    
    public Guid KeyFrameGuid { get; set; }

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
