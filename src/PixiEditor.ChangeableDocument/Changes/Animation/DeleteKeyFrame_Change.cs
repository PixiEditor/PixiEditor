using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class DeleteKeyFrame_Change : Change
{
    private readonly Guid _keyFrameId;
    private KeyFrame clonedKeyFrame;
    private KeyFrameData savedKeyFrameData;

    [GenerateMakeChangeAction]
    public DeleteKeyFrame_Change(Guid keyFrameId)
    {
        _keyFrameId = keyFrameId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.AnimationData.TryFindKeyFrame(_keyFrameId, out KeyFrame keyFrame))
        {
            Node node = target.FindNode<Node>(keyFrame.NodeId);
            if (node is null)
            {
                return false;
            }

            if(node.KeyFrames.FirstOrDefault()?.KeyFrameGuid == keyFrame.Id) // If the keyframe is the first one, we cannot delete it.
            {
                return false;
            }

            clonedKeyFrame = keyFrame.Clone();
            
            KeyFrameData data = node.KeyFrames.FirstOrDefault(x => x.KeyFrameGuid == keyFrame.Id);

            if (data is null)
            {
                return false;
            }

            savedKeyFrameData = data.Clone();
            
            return true;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        target.AnimationData.RemoveKeyFrame(_keyFrameId);
        target.FindNode<Node>(clonedKeyFrame.NodeId).RemoveKeyFrame(_keyFrameId);
        ignoreInUndo = false;
        return new DeleteKeyFrame_ChangeInfo(_keyFrameId);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.FindNode<Node>(clonedKeyFrame.NodeId).AddFrame(_keyFrameId, savedKeyFrameData.Clone());
        target.AnimationData.AddKeyFrame(clonedKeyFrame.Clone());
        List<IChangeInfo> changes =
        [
            new CreateRasterKeyFrame_ChangeInfo(clonedKeyFrame.NodeId, clonedKeyFrame.StartFrame, clonedKeyFrame.Id,
                false),

            new KeyFrameVisibility_ChangeInfo(clonedKeyFrame.Id, clonedKeyFrame.IsVisible),
            new KeyFrameLength_ChangeInfo(clonedKeyFrame.Id, clonedKeyFrame.StartFrame, clonedKeyFrame.Duration)
        ];

        return changes;
    }
}
