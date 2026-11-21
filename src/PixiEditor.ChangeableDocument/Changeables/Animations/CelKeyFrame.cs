using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class CelKeyFrame : KeyFrame, IReadOnlyCelKeyFrame
{
    public Document Document { get; set; }


    public CelKeyFrame(Guid keyFrameId, Guid nodeId, int startFrame, Document document)
        : base(nodeId, startFrame)
    {
        Id = keyFrameId;
        Document = document;
    }

    public override KeyFrame Clone()
    {
        return new CelKeyFrame(Id, NodeId, StartFrame, Document) { Duration = Duration, IsVisible = IsVisible };
    }
}
