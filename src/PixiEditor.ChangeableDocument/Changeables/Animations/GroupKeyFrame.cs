namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class GroupKeyFrame : KeyFrame
{
    private ChunkyImage originalLayerImage;
    private Document document;
    public List<KeyFrame> Children { get; } = new List<KeyFrame>();
    public override int Duration => Children.Count > 0 ? Children.Max(x => x.StartFrame + x.Duration) - StartFrame : 0;
    public override int StartFrame => Children.Count > 0 ? Children.Min(x => x.StartFrame) : 0;

    public GroupKeyFrame(Guid layerGuid, int startFrame, Document document) : base(layerGuid, startFrame)
    {
        Id = layerGuid;
        this.document = document;
        
        if (document.TryFindMember<RasterLayer>(LayerGuid, out var layer))
        {
            originalLayerImage = layer.LayerImage;
        }
    }

    public override void Deactivated(int atFrame)
    {
        //if(atFrame >= EndFrame) return;
        if (document.TryFindMember<RasterLayer>(LayerGuid, out var layer))
        {
           layer.LayerImage = originalLayerImage;
        }
    }

    public override bool IsWithinRange(int frame)
    {
        return frame >= StartFrame && frame < EndFrame + 1;
    }

    public override KeyFrame Clone()
    {
        var clone = new GroupKeyFrame(LayerGuid, StartFrame, document) { Id = this.Id };
        foreach (var child in Children)
        {
            clone.Children.Add(child.Clone());
        }

        return clone;
    }
}
