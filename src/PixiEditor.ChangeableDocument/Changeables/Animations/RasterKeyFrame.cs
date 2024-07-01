using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class RasterKeyFrame : KeyFrame, IReadOnlyRasterKeyFrame
{
    public ChunkyImage Image { get; set; }
    public Document Document { get; set; }
    
    IReadOnlyChunkyImage IReadOnlyRasterKeyFrame.Image => Image;
    
    public RasterKeyFrame(Guid targetLayerGuid, int startFrame, Document document, ChunkyImage? cloneFrom = null)
        : base(targetLayerGuid, startFrame)
    {
        Image = cloneFrom?.CloneFromCommitted() ?? new ChunkyImage(document.Size);

        Document = document;
    }
    
    public override KeyFrame Clone()
    {
        var image = Image.CloneFromCommitted();
        return new RasterKeyFrame(LayerGuid, StartFrame, Document, image) { Id = this.Id };
    }

    public override void Dispose()
    {
        Image.Dispose();
    }
}
