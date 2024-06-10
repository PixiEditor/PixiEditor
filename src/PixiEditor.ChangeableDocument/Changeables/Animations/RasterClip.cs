namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class RasterClip : Clip
{
    public Guid TargetLayerGuid { get; set; }
    public ChunkyImage Image { get; set; }
    public Document Document { get; set; }
    
    private ChunkyImage originalLayerImage;

    public RasterClip(Guid targetLayerGuid, int startFrame, Document document, ChunkyImage? cloneFrom = null)
    {
        TargetLayerGuid = targetLayerGuid;
        StartFrame = startFrame;
        
        Image = cloneFrom?.CloneFromCommitted() ?? new ChunkyImage(document.Size);

        Document = document;
    }
    
    public override void ActiveFrameChanged(int atFrame)
    {
        if (Document.TryFindMember<RasterLayer>(TargetLayerGuid, out var layer))
        {
            originalLayerImage = layer.LayerImage;
            layer.LayerImage = Image;
        }
    }

    public override void Deactivated(int atFrame)
    {
        if (Document.TryFindMember<RasterLayer>(TargetLayerGuid, out var layer))
        {
            layer.LayerImage = originalLayerImage;
        }
    }
}
