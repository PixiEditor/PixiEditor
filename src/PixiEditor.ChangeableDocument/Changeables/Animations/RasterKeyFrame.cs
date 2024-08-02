using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class RasterKeyFrame : KeyFrame, IReadOnlyRasterKeyFrame
{
    public Document Document { get; set; }

    private ImageLayerNode targetNode;

    public RasterKeyFrame(Guid id, ImageLayerNode node, int startFrame, Document document)
        : base(node, startFrame)
    {
        Id = id;
        targetNode = node;
        Document = document;
    }

    public override KeyFrame Clone()
    {
        return new RasterKeyFrame(Id, targetNode, StartFrame, Document) { Duration = Duration, IsVisible = IsVisible };
    }

    public IReadOnlyChunkyImage Image => targetNode.GetLayerImageByKeyFrameGuid(Id);
}
