using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class KeyFrameLength_UpdateableChange : UpdateableChange
{
    public Guid KeyFrameGuid { get; }
    public int StartFrame { get; private set; }
    public int Duration { get; private set; }

    private int originalStartFrame;
    private int originalDuration;

    private int originalNeighborStartFrame;
    private int originalNeighborDuration;
    private Guid? neighborKeyFrameGuid;

    [GenerateUpdateableChangeActions]
    public KeyFrameLength_UpdateableChange(Guid keyFrameGuid, int startFrame, int duration)
    {
        StartFrame = startFrame;
        Duration = duration;
        KeyFrameGuid = keyFrameGuid;
    }

    [UpdateChangeMethod]
    public void Update(int startFrame, int duration)
    {
        StartFrame = startFrame;
        Duration = duration;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.AnimationData.TryFindKeyFrame(KeyFrameGuid, out KeyFrame frame))
        {
            var node = target.FindNode<Node>(frame.NodeId);
            if (node is null)
            {
                return false;
            }

            if (node.KeyFrames.FirstOrDefault()?.KeyFrameGuid == frame.Id)
            {
                return false;
            }

            originalStartFrame = frame.StartFrame;
            originalDuration = frame.Duration;
            return true;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        target.AnimationData.TryFindKeyFrame(KeyFrameGuid, out KeyFrame keyFrame);

        var data = AdjustNeighbors(target, keyFrame, out var changeInfos);
        keyFrame.StartFrame = data.finalStartPos;
        keyFrame.Duration = data.finalDuration;
        changeInfos.Add(new KeyFrameLength_ChangeInfo(KeyFrameGuid, data.finalStartPos, data.finalDuration));

        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (originalStartFrame == StartFrame && originalDuration == Duration)
        {
            ignoreInUndo = true;
            return new None();
        }

        target.AnimationData.TryFindKeyFrame(KeyFrameGuid, out KeyFrame keyFrame);
        var data = AdjustNeighbors(target, keyFrame, out var changeInfos);

        keyFrame.StartFrame = data.finalStartPos;
        keyFrame.Duration = data.finalDuration;

        ignoreInUndo = false;
        changeInfos.Add(new KeyFrameLength_ChangeInfo(KeyFrameGuid, data.finalStartPos, data.finalDuration));

        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (originalStartFrame == StartFrame && originalDuration == Duration)
        {
            return new None();
        }

        target.AnimationData.TryFindKeyFrame(KeyFrameGuid, out KeyFrame keyFrame);
        keyFrame.StartFrame = originalStartFrame;
        keyFrame.Duration = originalDuration;

        List<IChangeInfo> changeInfos = new List<IChangeInfo>();
        if (neighborKeyFrameGuid.HasValue)
        {
            target.AnimationData.TryFindKeyFrame(neighborKeyFrameGuid.Value, out KeyFrame neighborKeyFrame);
            neighborKeyFrame.StartFrame = originalNeighborStartFrame;
            neighborKeyFrame.Duration = originalNeighborDuration;
            changeInfos.Add(new KeyFrameLength_ChangeInfo(neighborKeyFrameGuid.Value, originalNeighborStartFrame, originalNeighborDuration));
        }

        changeInfos.Add(new KeyFrameLength_ChangeInfo(KeyFrameGuid, originalStartFrame, originalDuration));
        return changeInfos;
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is KeyFrameLength_UpdateableChange change && change.KeyFrameGuid == KeyFrameGuid;
    }


    private (int finalStartPos, int finalDuration) AdjustNeighbors(Document target, KeyFrame keyFrame, out List<IChangeInfo> changeInfos)
    {
        int finalDuration = Duration;
        int finalStartFrame = StartFrame;
        changeInfos = new List<IChangeInfo>();
        bool toTheRight = StartFrame + Duration > keyFrame.StartFrame + keyFrame.Duration;
        if (toTheRight)
        {
            var firstKeyFrameToTheRight =
                target.AnimationData.TryGetNextKeyFrameAtFrame(keyFrame.NodeId,
                    keyFrame.StartFrame + keyFrame.Duration - 1);

            if (firstKeyFrameToTheRight != null && firstKeyFrameToTheRight.Id != KeyFrameGuid)
            {
                finalDuration = AdjustToTheRight(changeInfos, firstKeyFrameToTheRight, finalDuration);
            }
        }
        else
        {
            var firstKeyFrameToTheLeft =
                target.AnimationData.TryGetPreviousKeyFrameAtFrame(keyFrame.NodeId, keyFrame.StartFrame);

            if (firstKeyFrameToTheLeft != null && firstKeyFrameToTheLeft.Id != KeyFrameGuid)
            {
                (finalStartFrame, finalDuration) = AdjustToTheLeft(changeInfos, firstKeyFrameToTheLeft, finalStartFrame, finalDuration);
            }
        }

        return (finalStartFrame, finalDuration);
    }

    private (int finalStartFrame, int finalDuration) AdjustToTheLeft(List<IChangeInfo> changeInfos, KeyFrame firstKeyFrameToTheLeft, int finalStartFrame,
        int finalDuration)
    {
        if (neighborKeyFrameGuid == null)
        {
            neighborKeyFrameGuid = firstKeyFrameToTheLeft.Id;
            originalNeighborStartFrame = firstKeyFrameToTheLeft.StartFrame;
            originalNeighborDuration = firstKeyFrameToTheLeft.Duration;
        }

        int overlappingFrames = Math.Max((firstKeyFrameToTheLeft.StartFrame + firstKeyFrameToTheLeft.Duration) - StartFrame, 0);
        int newDuration = Math.Max(1, firstKeyFrameToTheLeft.Duration - overlappingFrames);
        newDuration = Math.Min(newDuration, originalNeighborDuration);
        firstKeyFrameToTheLeft.Duration = newDuration;

        changeInfos.Add(new KeyFrameLength_ChangeInfo(firstKeyFrameToTheLeft.Id,
            firstKeyFrameToTheLeft.StartFrame, newDuration));

        finalStartFrame = Math.Max(finalStartFrame, firstKeyFrameToTheLeft.StartFrame + firstKeyFrameToTheLeft.Duration);
        finalDuration = Math.Min(finalDuration, StartFrame + Duration - finalStartFrame);
        return (finalStartFrame, finalDuration);
    }

    private int AdjustToTheRight(List<IChangeInfo> changeInfos, KeyFrame firstKeyFrameToTheRight, int finalDuration)
    {
        if (neighborKeyFrameGuid == null)
        {
            neighborKeyFrameGuid = firstKeyFrameToTheRight.Id;
            originalNeighborStartFrame = firstKeyFrameToTheRight.StartFrame;
            originalNeighborDuration = firstKeyFrameToTheRight.Duration;
        }

        int overlappingFrames = Math.Max((StartFrame + Duration) - firstKeyFrameToTheRight.StartFrame, 0);
        int newDuration = Math.Max(1, firstKeyFrameToTheRight.Duration - overlappingFrames);
        newDuration = Math.Min(newDuration, originalNeighborDuration);
        int durationDifference = newDuration - firstKeyFrameToTheRight.Duration;
        int newStart = firstKeyFrameToTheRight.StartFrame + -durationDifference;
        firstKeyFrameToTheRight.StartFrame = newStart;
        firstKeyFrameToTheRight.Duration = newDuration;

        changeInfos.Add(new KeyFrameLength_ChangeInfo(firstKeyFrameToTheRight.Id, newStart,
            newDuration));

        finalDuration = Math.Min(finalDuration, firstKeyFrameToTheRight.StartFrame - StartFrame);
        return finalDuration;
    }
}
