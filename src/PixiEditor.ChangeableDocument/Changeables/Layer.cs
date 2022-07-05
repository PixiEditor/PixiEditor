using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Layer : StructureMember, IReadOnlyLayer
{
    // Don't forget to update CreateLayer_ChangeInfo, DocumentUpdater.ProcessCreateStructureMember and Layer.Clone when adding new properties
    public bool LockTransparency { get; set; } = false;
    public ChunkyImage LayerImage { get; set; }
    IReadOnlyChunkyImage IReadOnlyLayer.LayerImage => LayerImage;

    public Layer(VecI size)
    {
        LayerImage = new(size);
    }

    public Layer(ChunkyImage image)
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

    /// <summary>
    /// Creates a clone of the layer, its image and its mask
    /// </summary>
    /// <returns></returns>
    internal override Layer Clone()
    {
        return new Layer(LayerImage.CloneFromCommitted())
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
