using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class RasterKeyFrame : KeyFrame, IReadOnlyRasterKeyFrame
{
    public Document Document { get; set; }

    private ImageLayerNode targetNode;
    private ChunkyImage targetImage;

    public RasterKeyFrame(Guid id, ImageLayerNode node, int startFrame, Document document, ChunkyImage keyFrameImage)
        : base(node, startFrame)
    {
        Id = id;
        targetNode = node;
        targetImage = keyFrameImage;

        Document = document;
    }

    public override KeyFrame Clone()
    {
        var image = targetImage.CloneFromCommitted();
        return new RasterKeyFrame(Id, targetNode, StartFrame, Document, image) { Duration = Duration, IsVisible = IsVisible };
    }

    public IReadOnlyChunkyImage Image => targetImage;
}
