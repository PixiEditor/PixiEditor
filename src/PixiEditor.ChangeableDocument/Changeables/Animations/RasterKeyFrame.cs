using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class RasterKeyFrame : KeyFrame, IReadOnlyRasterKeyFrame
{
    public Document Document { get; set; }

    private RasterLayer targetLayer;
    private ChunkyImage targetImage;
    
    public RasterKeyFrame(Guid id, RasterLayer layer, int startFrame, Document document, ChunkyImage? cloneFrom = null)
        : base(layer.GuidValue, startFrame)
    {
        Id = id;
        targetLayer = layer;
        targetImage = cloneFrom?.CloneFromCommitted() ?? new ChunkyImage(document.Size);
        layer.AddFrame(Id, startFrame, 1, targetImage);

        Document = document;
    }
    
    public override KeyFrame Clone()
    {
        var image = targetImage.CloneFromCommitted();
        return new RasterKeyFrame(Id, targetLayer, StartFrame, Document, image);
    }

    public override void Dispose()
    {
    }

    public IReadOnlyChunkyImage Image => targetImage;
}
