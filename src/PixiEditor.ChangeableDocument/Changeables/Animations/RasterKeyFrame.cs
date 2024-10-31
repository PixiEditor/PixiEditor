using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class RasterKeyFrame : KeyFrame, IReadOnlyRasterKeyFrame
{
    public Document Document { get; set; }


    public RasterKeyFrame(Guid keyFrameId, Guid nodeId, int startFrame, Document document)
        : base(nodeId, startFrame)
    {
        Id = keyFrameId;
        Document = document;
    }

    public override KeyFrame Clone()
    {
        return new RasterKeyFrame(Id, NodeId, StartFrame, Document) { Duration = Duration, IsVisible = IsVisible };
    }

    public IReadOnlyChunkyImage GetTargetImage(IReadOnlyCollection<IReadOnlyNode> allNodes)
    {
        IReadOnlyNode owner = allNodes.First(x => x.Id == NodeId);
        if(owner is ImageLayerNode imageLayer)
        {
            return imageLayer.GetLayerImageByKeyFrameGuid(Id);
        }

        throw new InvalidOperationException("Node is not an image layer");
    }
}
