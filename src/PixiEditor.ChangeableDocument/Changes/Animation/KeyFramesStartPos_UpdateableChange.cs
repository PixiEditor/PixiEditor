using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class KeyFramesStartPos_UpdateableChange : InterruptableUpdateableChange
{
    public Guid[] KeyFramesGuid { get; }
    public int Delta { get; set; }

    private Dictionary<Guid, int> originalStartFrames = new();

    [GenerateUpdateableChangeActions]
    public
        KeyFramesStartPos_UpdateableChange(List<Guid> keyFramesGuid,
            int delta) // do not delete, code generator uses this for update method
    {
        KeyFramesGuid = keyFramesGuid.ToArray();
        Delta = 0;
    }

    [UpdateChangeMethod]
    public void Update(int delta)
    {
        Delta += delta;
    }

    public override bool InitializeAndValidate(Document target)
    {
        foreach (Guid keyFrameGuid in KeyFramesGuid)
        {
            if (target.AnimationData.TryFindKeyFrame(keyFrameGuid, out KeyFrame keyFrame))
            {
                var node = target.FindNode<Node>(keyFrame.NodeId);
                if (node is null)
                {
                    return false;
                }

                if (node.KeyFrames.FirstOrDefault()?.KeyFrameGuid == keyFrameGuid)
                {
                    return false;
                }

                originalStartFrames[keyFrameGuid] = keyFrame.StartFrame;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        List<IChangeInfo> changes = new();
        foreach (Guid keyFrameGuid in KeyFramesGuid)
        {
            target.AnimationData.TryFindKeyFrame(keyFrameGuid, out KeyFrame keyFrame);
            keyFrame.StartFrame = originalStartFrames[keyFrameGuid] + Delta;
            changes.Add(new KeyFrameLength_ChangeInfo(keyFrameGuid, keyFrame.StartFrame, keyFrame.Duration));
        }

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        List<IChangeInfo> changes = new();
        foreach (Guid keyFrameGuid in KeyFramesGuid)
        {
            target.AnimationData.TryFindKeyFrame(keyFrameGuid, out KeyFrame keyFrame);
            keyFrame.StartFrame = originalStartFrames[keyFrameGuid] + Delta;
            changes.Add(new KeyFrameLength_ChangeInfo(keyFrameGuid, keyFrame.StartFrame, keyFrame.Duration));
        }

        ignoreInUndo = false;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new();
        foreach (Guid keyFrameGuid in KeyFramesGuid)
        {
            target.AnimationData.TryFindKeyFrame(keyFrameGuid, out KeyFrame keyFrame);
            keyFrame.StartFrame = originalStartFrames[keyFrameGuid];
            changes.Add(new KeyFrameLength_ChangeInfo(keyFrameGuid, keyFrame.StartFrame, keyFrame.Duration));
        }

        return changes;
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is KeyFramesStartPos_UpdateableChange otherChange &&
               otherChange.KeyFramesGuid.SequenceEqual(KeyFramesGuid);
    }
}
