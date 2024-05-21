using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class RasterLayer : Layer, IReadOnlyRasterLayer
{
    // Don't forget to update CreateLayer_ChangeInfo, DocumentUpdater.ProcessCreateStructureMember and Layer.Clone when adding new properties
    public bool LockTransparency { get; set; } = false;
    public ChunkyImage LayerImage { get; set; }
    IReadOnlyChunkyImage IReadOnlyRasterLayer.LayerImage => LayerImage;

    public RasterLayer(VecI size)
    {
        LayerImage = new(size);
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
    }

    public override ChunkyImage Rasterize()
    {
        return LayerImage;
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
