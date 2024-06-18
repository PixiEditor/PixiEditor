namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class RasterKeyFrame : KeyFrame
{
    public ChunkyImage Image { get; set; }
    public Document Document { get; set; }
    
    public RasterKeyFrame(Guid targetLayerGuid, int startFrame, Document document, ChunkyImage? cloneFrom = null)
        : base(targetLayerGuid, startFrame)
    {
        Image = cloneFrom?.CloneFromCommitted() ?? new ChunkyImage(document.Size);

        Document = document;
    }
    
    public override void ActiveFrameChanged(int atFrame)
    {
        if (Document.TryFindMember<RasterLayer>(LayerGuid, out var layer))
        {
            layer.LayerImage = Image;
        }
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
