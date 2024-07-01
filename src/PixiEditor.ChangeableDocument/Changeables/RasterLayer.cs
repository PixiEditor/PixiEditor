using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class RasterLayer : Layer, IReadOnlyRasterLayer
{
    // Don't forget to update CreateLayer_ChangeInfo, DocumentUpdater.ProcessCreateStructureMember and Layer.Clone when adding new properties
    public bool LockTransparency { get; set; } = false;

    public ChunkyImage LayerImage
    {
         get
    }
    IReadOnlyChunkyImage IReadOnlyRasterLayer.LayerImage => LayerImage;
    IReadOnlyChunkyImage IChunkyImageProperty.LayerImage => LayerImage;
    
    private List<ImageFrame> frameImages = new();
    
    public RasterLayer(VecI size)
    {
        frameImages.Add(new ImageFrame(0, 0, new(size)));
    }

    public RasterLayer(ChunkyImage image)
    {
        LayerImage = image;
    }

    /// <summary>
    /// Disposes the layer's image and mask
    /// </summary>
    public override void Dispose()
    {
        LayerImage.Dispose();
        Mask?.Dispose();
        foreach (var frame in frameImages)
        {
            frame.Image.Dispose();
        }
    }

    public override ChunkyImage Rasterize(KeyFrameTime frameTime)
    {
        if (frameImages.Count == 0)
        {
            return LayerImage;
        }
        
        ImageFrame frame = frameImages.FirstOrDefault(x => x.IsInFrame(frameTime.Frame));
        
        return frame?.Image ?? LayerImage;
    }

    public override RectI? GetTightBounds()
    {
        return LayerImage.FindTightCommittedBounds();
    }

    /// <summary>
    /// Creates a clone of the layer, its image and its mask
    /// </summary>
    internal override RasterLayer Clone()
    {
        return new RasterLayer(LayerImage.CloneFromCommitted())
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
}

class ImageFrame
{
    int StartFrame { get; set; }
    int EndFrame { get; set; }
    public ChunkyImage Image { get; set; }
    
    public ImageFrame(int startFrame, int endFrame, ChunkyImage image)
    {
        StartFrame = startFrame;
        EndFrame = endFrame;
        Image = image;
    }
    
    public bool IsInFrame(int frame)
    {
        return frame >= StartFrame && frame <= EndFrame;
    }
}
