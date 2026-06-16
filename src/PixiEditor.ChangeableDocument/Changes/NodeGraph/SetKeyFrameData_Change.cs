using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class SetKeyFrameData_Change : Change
{
    private Guid nodeId;
    private Guid keyFrameId;
    private object data;
    private int startFrame;
    private int duration;
    private string affectedElement;
    private bool isVisible;

    [GenerateMakeChangeAction]
    public SetKeyFrameData_Change(Guid nodeId, Guid keyFrameId, object data, int startFrame, int duration,
        string affectedElement, bool isVisible)
    {
        this.nodeId = nodeId;
        this.keyFrameId = keyFrameId;
        this.data = data;
        this.startFrame = startFrame;
        this.duration = duration;
        this.affectedElement = affectedElement;
        this.isVisible = isVisible;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindNode(nodeId, out Node node);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        Node node = target.FindNode(nodeId);

        KeyFrameData keyFrame = node.KeyFrames.FirstOrDefault(
            x => x.KeyFrameGuid == keyFrameId
                 || IsSpecialRootKeyFrame(x));

        var newKeyFrame = new KeyFrameData(keyFrameId, startFrame, duration, affectedElement)
        {
            Data = data,
            IsVisible = isVisible
        };

        if (keyFrame != null)
        {
            node.RemoveKeyFrame(keyFrame.KeyFrameGuid);
        }
         
        node.AddFrame(keyFrameId, newKeyFrame);

        ignoreInUndo = false;

        return new None();
    }

    private bool IsSpecialRootKeyFrame(KeyFrameData x)
    {
        return (x is { StartFrame: 0, Duration: 0 } && startFrame == 0 && duration == 0 &&
                x.AffectedElement == affectedElement);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        throw new InvalidOperationException(
            "Cannot revert SetKeyFrameData_Change, this change is only meant for setting key frame data.");
        return new None(); // do not remove, code generator doesn't work without it 
    }
}
